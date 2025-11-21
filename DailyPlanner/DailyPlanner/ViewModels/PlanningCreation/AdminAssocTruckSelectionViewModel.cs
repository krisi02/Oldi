using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using DailyPlanner.ViewModels.PlanningCreation;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DailyPlanner.ViewModels
{
    public class AdminAssocTruckSelectionViewModel : PN_BaseViewModel, IInitialize
    {
        IRepository repository;
        INavigationService navigationService;
        IPageDialogService dialogService;

        public AdminAssocTruckSelectionViewModel(IRepository _repository, INavigationService _navgationService, IPageDialogService _dialogService) :
            base(_navgationService, _repository)
        {

            navigationService = _navgationService;
            repository = _repository;
            dialogService = _dialogService;

            BindCommands();
        }

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.SELECTED_PLANNING_DATE_KEY))
            {
                selectedPlanningDate = parameters.GetValue<DateTime>(Constants.SELECTED_PLANNING_DATE_KEY);
            }
            LoadData();
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
                        PlanningTrucks = new ObservableCollection<TruckCardViewModel>();
                        PlanningDrivers = new ObservableCollection<DriverCardViewModel>();
                        foreach (var item in truckDriverMatrixResponse.Item.Matrix)
                        {
                            if (Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL)
                            {
                                if (item.Driver.DriverCode == "-1" && item.Truck.TruckCode != -1)
                                {
                                    PlanningTrucks.Add(new TruckCardViewModel(item.Truck));
                                }
                                if (item.Driver.DriverCode != "-1" && item.Truck.TruckCode == -1)
                                {
                                    PlanningDrivers.Add(new DriverCardViewModel(item.Driver));
                                }
                            }
                        }
                        RaisePropertyChanged(nameof(PlanningTrucks));
                        ReleaseView();
                    }
                }

            }
            catch (Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
                NavigationParameters p = new NavigationParameters();
                p.Add(Constants.EXIT_KEY, true);
                await navigationService.GoBackAsync(p);
            }
        }

        private void BindCommands()
        {
            SelectedTruck = new DelegateCommand<TruckCardViewModel>(OnTruckSelected);
        }

        private async void OnTruckSelected(TruckCardViewModel obj)
        {
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(Constants.SELECTED_TRUCK_KEY, obj);
            navigationParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, selectedPlanningDate);
            navigationParameters.Add(Constants.LOADED_PLANNING_MATRIX_KEY, PlanningDrivers);
            try
            {
                var a = await navigationService.NavigateAsync("AdminAssocDriverSelection", navigationParameters);
            }
            catch (Exception x)
            {

            }
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
        public ObservableCollection<TruckCardViewModel> PlanningTrucks { get; set; }
        public ObservableCollection<DriverCardViewModel> PlanningDrivers { get; set; }
        public ICommand SelectedTruck { get; set; }
    }

}
