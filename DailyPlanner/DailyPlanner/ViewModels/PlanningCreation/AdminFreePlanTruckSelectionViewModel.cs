using DailyPlanner.Models;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DailyPlanner.ViewModels.PlanningCreation
{
    class AdminFreePlanTruckSelectionViewModel : PN_BaseViewModel, IInitialize
    {
        IRepository repository;
        INavigationService navigationService;
        IPageDialogService dialogService;

        public AdminFreePlanTruckSelectionViewModel(IRepository _repository, INavigationService _navgationService, IPageDialogService _dialogService) :
            base(_navgationService, _repository)
        {

            navigationService = _navgationService;
            repository = _repository;
            dialogService = _dialogService;

            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName.Equals(nameof(SearchFilter)))
                {
                    ConfigTruckListView();
                }
            };
            BindCommands();
        }


        private void BindCommands()
        {
            SelectedTruckDriver = new DelegateCommand<TruckDriverCardViewModel>(OnTruckSelected);
            ChangeView = new DelegateCommand(OnChangeViewPressed);
        }

        private void OnChangeViewPressed()
        {
            if (!IsAdmin) { return; }
            BlockView();
            if (_viewType == 0) { _viewType = 1; } else { _viewType = 0; }
            ConfigTruckListView();
            ReleaseView();

        }


        private async void OnTruckSelected(TruckDriverCardViewModel obj)
        {
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(Constants.SELECTED_TRUCK_DRIVER_KEY, obj);
            navigationParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, selectedPlanningDate);
            try
            {
                    var a = await navigationService.NavigateAsync("AdminFreePlanPlanning", navigationParameters);
            }
            catch (Exception x)
            {

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
                        _searchFilter = "";
                        _truckDriverMatrix = truckDriverMatrixResponse.Item;
                        ConfigTruckListView();
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


        private void ConfigTruckListView()
        {
            PlanningTruckDriverList = new ObservableCollection<TruckDriverCardViewModel>();
            IOrderedEnumerable<TruckDriverModel> truckDriverList;
            if (_viewType == 1)
            {
                truckDriverList = _truckDriverMatrix.Matrix.Where(x => x.Truck.TruckInternalNumber.ToUpper().Contains(SearchFilter.ToUpper()) && x.Truck.CompanyTruck == -1).OrderBy(x => x.Truck.PlantDescription);
            }
            else
            {
                truckDriverList = _truckDriverMatrix.Matrix.Where(x => x.Truck.TruckInternalNumber.ToUpper().Contains(SearchFilter.ToUpper()) && x.Truck.CompanyTruck == 0).OrderBy(x => x.Truck.TruckInternalNumber);
            }
            foreach (var item in truckDriverList)
            {
                if(item.Driver.DriverCode == "-1")
                {
                    item.Driver.DriverName = "**NO AUTISTA**";
                }
                if (item.Truck.PlantDescription.Trim() == "")
                {
                    item.Truck.PlantDescription = "**JOLLY**";
                }
                PlanningTruckDriverList.Add(new TruckDriverCardViewModel(item));
            }
            RaisePropertyChanged(nameof(PlanningTruckDriverList));
            RaisePropertyChanged(nameof(ViewTypeDescription));
        }

        private string _searchFilter = "";
        public string SearchFilter
        {
            get { return _searchFilter; }
            set { SetProperty(ref _searchFilter, value); }
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
        //0-> Only Company 1-> Others
        private int _viewType { get; set; }

        public string ViewTypeDescription { get { return _viewType == 0 ? "MEZZI AZIENDALI" : "MEZZI PADRONCINI"; } }
        public bool IsAdmin { get { return Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL; } }
        public ObservableCollection<TruckDriverCardViewModel> PlanningTruckDriverList { get; set; }
        public ICommand SelectedTruckDriver { get; set; }
        public ICommand ChangeView { get; set; }

    }
}
