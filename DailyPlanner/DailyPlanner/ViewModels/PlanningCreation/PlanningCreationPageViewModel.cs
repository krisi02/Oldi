using DailyPlanner.Models;
using DailyPlanner.Models.NetworkModels;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using DailyPlanner.ViewModels.ListViewItems;
using DailyPlanner.ViewModels.PlanningCreation;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DailyPlanner.ViewModels
{
    public class PlanningCreationPageViewModel : PN_BaseViewModel, IInitialize
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        public PlanningCreationPageViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService)
            :base(_navigationService, _repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            BindCommands();
        }

        private void BindCommands()
        {
            OnChangePlantChange = new DelegateCommand(() => {
                RaisePropertyChanged(nameof(ChangePlant));
            });
            OnAdminIsRentalChange = new DelegateCommand(() => {
                RaisePropertyChanged(nameof(AdminIsRental));
            });
            SaveNewPlanning = new DelegateCommand( async () =>
            {
                bool userResponse = await dialogService.DisplayAlertAsync("RICHIESTA CONFERMA", "Procedere con il salvataggio dei dati ?", "PROCEDI", "CHIUDI");
                if (!userResponse) { return; }
                BlockView();
                if (IsAdmin)
                {
                    AdminPlanningCreation();
                }
                else
                {
                    ImpiantistaPlanningCreation();
                }
            });
        }

        private async void ImpiantistaPlanningCreation()
        {
            Planning planning = CollectImpiantistaDataForCreation();
            try
            {
                if (_isModify)
                {
                    planning.Id = selectedPlanning.Id;
                    var result = await repository.UpdatePlanning(planning);
                    ReleaseView();
                    ContinueWithExecution(result);
                }
                else
                {
                    var result = await repository.CreatePlanning(planning);
                    ReleaseView();
                    ContinueWithExecution(result);
                }
            }
            catch (Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
            }
        }

        private async void AdminPlanningCreation()
        {
            Planning planning = CollectAdminDataForCreation();
            try
            {
                var result = await repository.CreatePlanning(planning);
                ReleaseView();
                ContinueWithExecution(result);
            }
            catch (Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
            }
        }

        private async void ContinueWithExecution(Response<bool> result)
        {
            if (result.Success)
            {
                NavigationParameters p = new NavigationParameters();
                p.Add(Constants.EXIT_KEY, true);
                await navigationService.GoBackAsync(p);
            }
            else
            {
                string errorMessage = result.Message == null ? "Errore imprevisto durante il salvataggio!" : result.Message;
                await dialogService.DisplayAlertAsync("Errore", errorMessage, "OK");
            }
        }

      
        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.SELECTED_TRUCK_DRIVER_KEY))
            {
                LoadData(parameters);
            }
        }

        private async void LoadData(INavigationParameters parameters)
        {
            _selectedTruckDriverCard = parameters.GetValue<TruckDriverCardViewModel>(Constants.SELECTED_TRUCK_DRIVER_KEY);
            _isNewAssoc = parameters.GetValue<int>(Constants.SELECTED_PLANNING_W_ASSOC_KEY);
            RaisePropertyChanged(nameof(TruckTypeColor));
            RaisePropertyChanged(nameof(TruckInternalNumber));
            RaisePropertyChanged(nameof(TruckTypeDescription));
            RaisePropertyChanged(nameof(Description));

            selectedPlanningDate = parameters.GetValue<DateTime>(Constants.SELECTED_PLANNING_DATE_KEY).Date;
            RaisePropertyChanged(nameof(SelectedDateString));

            StartingTime = DateTime.Now.TimeOfDay;

            _isModify = parameters.GetValue<bool>(Constants.IS_MODIFY_KEY);
            if (_isModify)
            {
                selectedPlanning = parameters.GetValue<Planning>(Constants.SELECTED_PLANNING_KEY);
                StartingTime = selectedPlanning.PlanningStartingTime.TimeOfDay;
                Note = selectedPlanning.Note;
                RaisePropertyChanged(nameof(StartingTime));
                RaisePropertyChanged(nameof(Note));
            }
            try
            {
                bool retry = true;
                while (retry)
                {
                    var plantListResp = await repository.GetPlantList();
                    retry = false;
                    if (!plantListResp.Success)
                    {
                        if (IsAuthorized(plantListResp.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{plantListResp.Message}", "RIPROVA ", "CHIUDI");
                            if (!retry)
                            {
                                NavigationParameters p = new NavigationParameters();
                                p.Add(Constants.EXIT_KEY, true);
                                await navigationService.GoBackAsync(p);
                            }
                        }
                        else
                        {
                            await ForceLogout();
                            ReleaseView();
                            await dialogService.DisplayAlertAsync("Errore", $"Sessione scaduta! Effettuare nuovamente il login!", "OK");
                            await navigationService.NavigateAsync("/LoginPage");
                        }
                    }
                    else
                    {
                        ReleaseView();
                        PlantList = new ObservableCollection<Plant>();
                        foreach (var item in plantListResp.Item.Plants)
                        {
                            PlantList.Add(item);
                        }
                        RaisePropertyChanged(nameof(PlantList));
                    }

                }
            }
            catch (Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
            }
        }


        private Planning CollectImpiantistaDataForCreation()
        {
            Planning planningData = new Planning();
            planningData.Id = -1;
            planningData.AssignedTruck = _selectedTruckDriverCard.TruckDriverModel.Truck;
            planningData.Day = selectedPlanningDate;
            planningData.PlannedDriver = _selectedTruckDriverCard.TruckDriverModel.Driver;
            if (ChangePlant)
            {
                //If a different plant is selected than assign it
                planningData.PlannedPlantCode = _selectedPlant.PlantCode;
                planningData.PlannedPlantName = _selectedPlant.PlantDescription;
            }
            else
            {
                planningData.PlannedPlantCode = Settings.Settings.Default.PlantCode;
            }
            planningData.Note = Note == null ? "": Note;
            planningData.PlanningStartingTime = selectedPlanningDate.Date.Add(StartingTime);
            planningData.UserId = Settings.Settings.Default.LoggedUserId;
            planningData.NewAssoc = 0;
            return planningData;
        }
        private Planning CollectAdminDataForCreation()
        {
            Planning planningData = new Planning();
            planningData.Id = -1;
            planningData.AssignedTruck = _selectedTruckDriverCard.TruckDriverModel.Truck;
            planningData.Day = selectedPlanningDate;
            planningData.PlannedDriver = _selectedTruckDriverCard.TruckDriverModel.Driver;
            planningData.PlannedPlantCode = _adminSelectedPlant.PlantCode;
            planningData.PlannedPlantName = _adminSelectedPlant.PlantDescription;
            planningData.Note = Note == null ? "" : Note;
            planningData.IsRental = AdminIsRental ? 1: 0;
            planningData.PlanningStartingTime = selectedPlanningDate.Date.Add(StartingTime);
            planningData.UserId = Settings.Settings.Default.LoggedUserId;
            planningData.NewAssoc = _isNewAssoc;
            return planningData;
        }

        private void BlockView()
        {
            CanNavigate = false;
            IsBusy = true;
        }
        private void ReleaseView()
        {
            CanNavigate = true;
            IsBusy = false;
        }

        #region READONLY

        private TruckDriverCardViewModel _selectedTruckDriverCard { get; set; }
        public string TruckTypeColor { get { return _selectedTruckDriverCard == null ? "" : _selectedTruckDriverCard.TruckTypeColor; } }
        public string TruckInternalNumber { get { return _selectedTruckDriverCard == null ? "" : _selectedTruckDriverCard.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return _selectedTruckDriverCard == null ? "" : _selectedTruckDriverCard.TruckTypeDescription; } }
        public string Description { get { return _selectedTruckDriverCard == null ? "" : _selectedTruckDriverCard.Description; } }

        public string SelectedDateString { get { return selectedPlanningDate.DayOfWeek.ToString() + " " + selectedPlanningDate.ToString("dd/MM/yyyy"); } }

        #endregion

        #region PLANNING DATA
        private TimeSpan _startingTime { get; set; }
        public TimeSpan StartingTime { get { return _startingTime; } set { _startingTime = value; RaisePropertyChanged(nameof(StartingTime)); } }
        
        private string _note { get; set; }
        public string Note { get { return _note; } set { _note = value; RaisePropertyChanged(nameof(Note)); } }
        
        private bool _changePlant { get; set; }
        public bool ChangePlant { get { return _changePlant; } set { _changePlant = value; RaisePropertyChanged(nameof(ChangePlant)); } }

        public ObservableCollection<Plant> PlantList { get; set; }
        private Plant _selectedPlant { get; set; }
        public Plant SelectedPlant { get { return _selectedPlant; } set { _selectedPlant = value; } }
        private Plant _adminSelectedPlant { get; set; }
        public Plant AdminSelectedPlant { get { return _adminSelectedPlant; } set { _adminSelectedPlant = value; } }

        private bool _adminIsRental { get; set; }
        public bool AdminIsRental { get { return _adminIsRental; } set { _adminIsRental = value; RaisePropertyChanged(nameof(AdminIsRental)); } }

        #endregion

        private bool _isModify { get; set; }
        private int _isNewAssoc { get; set; }


        public bool IsImpiantista { get { return Settings.Settings.Default.LoggedUserRole != Models.Enum.RolesEnum.NEL; } }
        public bool IsAdmin { get { return Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL;  } }

        public ICommand OnChangePlantChange { get; set; }
        public ICommand OnAdminIsRentalChange { get; set; }
        public ICommand SaveNewPlanning { get; set; }
    }
}
