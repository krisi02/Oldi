using DailyPlanner.Models;
using DailyPlanner.Models.Events;
using DailyPlanner.Models.NetworkModels;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace DailyPlanner.ViewModels.PlanningCreation
{
    public class AdminFreePlanPlanningViewModel : PN_BaseViewModel, IInitialize
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        IEventAggregator eventAggregator;

        public AdminFreePlanPlanningViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService, IEventAggregator _eventAggregator)
            : base(_navigationService, _repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            eventAggregator = _eventAggregator;
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
            SaveNewPlanning = new DelegateCommand(async () =>
            {
                bool userResponse = await dialogService.DisplayAlertAsync("RICHIESTA CONFERMA", "Procedere con il salvataggio dei dati ?", "PROCEDI", "CHIUDI");
                if (!userResponse) { return; }
                BlockView();
                if (IsAdmin)
                {
                    if (_adminSelectedPlant == null)
                    {
                        await dialogService.DisplayAlertAsync("Errore", "Selezionare un impianto!", "CHIUDI");
                        ReleaseView();
                        return;
                    }
                    AdminPlanningCreation();
                }
            });
            OnChangeDriverCommand = new DelegateCommand(async () =>
            {
                eventAggregator.GetEvent<AdminFreePlanDriverSelected>().Subscribe(DriverChanged);
                var navigationParameters = new NavigationParameters();
                navigationParameters.Add(Constants.SELECTED_TRUCK_KEY, _selectedTruckDriverCard);
                navigationParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, selectedPlanningDate);
                var a = await navigationService.NavigateAsync("AdminFreePlanDriverSelection", navigationParameters);
            });
        }

        private  void DriverChanged(DriverModel obj)
        {
            _selectedTruckDriverCard.TruckDriverModel.Driver = obj;
            RaisePropertyChanged(nameof(TruckTypeColor));
            RaisePropertyChanged(nameof(TruckInternalNumber));
            RaisePropertyChanged(nameof(TruckTypeDescription));
            RaisePropertyChanged(nameof(Description));
            eventAggregator.GetEvent<AdminFreePlanDriverSelected>().Unsubscribe(DriverChanged);
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
            _isNewAssoc = 0;
            RaisePropertyChanged(nameof(TruckTypeColor));
            RaisePropertyChanged(nameof(TruckInternalNumber));
            RaisePropertyChanged(nameof(TruckTypeDescription));
            RaisePropertyChanged(nameof(Description));
            AdminIsRental = true;
            selectedPlanningDate = parameters.GetValue<DateTime>(Constants.SELECTED_PLANNING_DATE_KEY).Date;
            RaisePropertyChanged(nameof(SelectedDateString));

            StartingTime = DateTime.Now.TimeOfDay;
            /*
            _isModify = parameters.GetValue<bool>(Constants.IS_MODIFY_KEY);
            if (_isModify)
            {
                selectedPlanning = parameters.GetValue<Planning>(Constants.SELECTED_PLANNING_KEY);
                StartingTime = selectedPlanning.PlanningStartingTime.TimeOfDay;
                Note = selectedPlanning.Note;
                RaisePropertyChanged(nameof(StartingTime));
                RaisePropertyChanged(nameof(Note));
            }*/
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
            planningData.IsRental = AdminIsRental ? 1 : 0;
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
        private Plant _selectedPlant { get; 
            set; }
        public Plant SelectedPlant { get { return _selectedPlant; } 
            set { _selectedPlant = value; } }
        private Plant _adminSelectedPlant { get; 
            set; }
        public Plant AdminSelectedPlant { get { return _adminSelectedPlant; } 
            set { _adminSelectedPlant = value; } }

        private bool _adminIsRental { get; set; }
        public bool AdminIsRental { get { return _adminIsRental; } set { _adminIsRental = value; RaisePropertyChanged(nameof(AdminIsRental)); } }

        #endregion

        private bool _isModify { get; set; }
        private int _isNewAssoc { get; set; }


        public bool IsImpiantista { get { return Settings.Settings.Default.LoggedUserRole != Models.Enum.RolesEnum.NEL; } }
        public bool IsAdmin { get { return Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL; } }

        public ICommand OnChangeDriverCommand { get; set; }
        public ICommand OnChangePlantChange { get; set; }
        public ICommand OnAdminIsRentalChange { get; set; }
        public ICommand SaveNewPlanning { get; set; }
    }
}
