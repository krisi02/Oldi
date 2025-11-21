using DailyPlanner.Models;
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
    public class PlanningTruckDriverSelectionPageViewModel : PN_BaseViewModel, IInitialize
    {
        IRepository repository;
        INavigationService navigationService;
        IPageDialogService dialogService;
        public PlanningTruckDriverSelectionPageViewModel(IRepository _repository, INavigationService _navgationService, IPageDialogService _dialogService) :
            base(_navgationService, _repository)
        {
            navigationService = _navgationService;
            repository = _repository;
            dialogService = _dialogService;

            BindCommands();
        }

        private void BindCommands()
        {
            SelectedTruckDriver = new DelegateCommand<TruckDriverCardViewModel>(OnTruckDriverSelected);
            ChangeView = new DelegateCommand(OnChangeViewPressed);
        }

        private void OnChangeViewPressed()
        {
            if (!IsAdmin) { return; }
            BlockView();
            if(_viewType == 0) { _viewType = 1; } else { _viewType = 0; }
            PlanningTruckDriverList = new ObservableCollection<TruckDriverCardViewModel>();
            IOrderedEnumerable<TruckDriverModel> truckDriverList;
            if (_viewType == 1)
            {
                truckDriverList = _truckDriverMatrix.Matrix.OrderBy(x => x.Truck.PlantDescription);
            }
            else
            {
                truckDriverList = _truckDriverMatrix.Matrix.OrderBy(x=> x.Truck.TruckInternalNumber);
            }
            foreach (var item in truckDriverList)
            {
                if (item.Driver.DriverCode != "-1" && item.Truck.TruckCode != -1)
                {
                    if(_viewType == 0)
                    {
                        if(item.Truck.PlantDescription.Trim() == "" || item.Truck.PlantDescription.Trim() == "JOLLY")
                        {
                            item.Truck.PlantDescription = "JOLLY";
                            PlanningTruckDriverList.Add(new TruckDriverCardViewModel(item));
                        }
                    }
                    else
                    {
                        
                        if (item.Truck.PlantDescription.Trim() != "" && item.Truck.PlantDescription.Trim() != "JOLLY")
                        {
                            PlanningTruckDriverList.Add(new TruckDriverCardViewModel(item));
                        }
                    }
                }
            }
            RaisePropertyChanged(nameof(PlanningTruckDriverList));
            ReleaseView();
        }

        private async void OnTruckDriverSelected(TruckDriverCardViewModel obj)
        {
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(Constants.SELECTED_TRUCK_DRIVER_KEY, obj);
            navigationParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, selectedPlanningDate);
            try
            {
                var a = await navigationService.NavigateAsync("PlanningCreationPage", navigationParameters);
            }
            catch (Exception x)
            {

            }

        }

        

        private async void LoadData()
        {
            BlockView();
            try
            {
                bool retry = true;
                while (retry)
                {
                    var truckDriverMatrixResponse = await repository.GetTruckDriverMatrixForPlanning(selectedPlanningDate);
                    retry = false;
                    if (!truckDriverMatrixResponse.Success)
                    {
                        if (IsAuthorized(truckDriverMatrixResponse.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{truckDriverMatrixResponse.Message}", "RIPROVA ", "CHIUDI");
                            if (!retry) {
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
                        PlanningTruckDriverList = new ObservableCollection<TruckDriverCardViewModel>();
                        foreach (var item in truckDriverMatrixResponse.Item.Matrix)
                        {
                            if(Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL)
                            {
                                if(item.Driver.DriverCode != "-1" && item.Truck.TruckCode != -1 && item.Truck.PlantDescription.Trim() == "")
                                {
                                    item.Truck.PlantDescription = "JOLLY";
                                    PlanningTruckDriverList.Add(new TruckDriverCardViewModel(item));
                                }
                            }
                            else
                            {
                                PlanningTruckDriverList.Add(new TruckDriverCardViewModel(item));
                            }
                        }
                        _viewType = 0;
                        _truckDriverMatrix = truckDriverMatrixResponse.Item;
                        RaisePropertyChanged(nameof(PlanningTruckDriverList));
                        ReleaseView();
                    }
                }
                
            }
            catch(Exception x)
            {
                ReleaseView();
                await dialogService.DisplayActionSheetAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
                NavigationParameters p = new NavigationParameters();
                p.Add(Constants.EXIT_KEY, true);
                await navigationService.GoBackAsync(p);
            }
        }


        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.SELECTED_PLANNING_DATE_KEY))
            {
                selectedPlanningDate = parameters.GetValue<DateTime>(Constants.SELECTED_PLANNING_DATE_KEY);
            }
            LoadData();
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

        private TruckDriverMatrix _truckDriverMatrix { get; set; }
        //0-> Only Jolly 1-> Others
        private int _viewType { get; set; }
        public bool IsAdmin { get { return Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL; } }
        public ObservableCollection<TruckDriverCardViewModel> PlanningTruckDriverList { get; set; }
        public ICommand SelectedTruckDriver { get; set; }
        public ICommand ChangeView { get; set; }
    }
}
