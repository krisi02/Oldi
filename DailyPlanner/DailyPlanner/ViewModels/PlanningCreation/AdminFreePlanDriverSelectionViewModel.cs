using DailyPlanner.Models;
using DailyPlanner.Models.Events;
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
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DailyPlanner.ViewModels.PlanningCreation
{

    public class AdminFreePlanDriverSelectionViewModel : PN_BaseViewModel, IInitialize
    {
        IRepository repository;
        INavigationService navigationService;
        IPageDialogService dialogService;
        IEventAggregator eventAggregator;

        public AdminFreePlanDriverSelectionViewModel(IRepository _repository, INavigationService _navgationService, IPageDialogService _dialogService, IEventAggregator _eventAggregator) :
            base(_navgationService, _repository)
        {
            navigationService = _navgationService;
            repository = _repository;
            dialogService = _dialogService;
            eventAggregator = _eventAggregator;
            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName.Equals(nameof(SearchFilter)))
                {
                    ConfigDriverListView();
                }
            };
            BindCommands();
        }
        private void BindCommands()
        {
            SelectedDriver = new DelegateCommand<DriverCardViewModel>(OnDriverSelected);
            ChangeView = new DelegateCommand(OnChangeViewPressed);
        }

        private void OnChangeViewPressed()
        {
            BlockView();
            if (_viewType == 0) { _viewType = 1; } else { _viewType = 0; }
            ConfigDriverListView();
            ReleaseView();

        }

        private async void OnDriverSelected(DriverCardViewModel obj)
        {
            eventAggregator.GetEvent<AdminFreePlanDriverSelected>().Publish(obj.Driver);
            await navigationService.GoBackAsync();
            /*
            var paramObj = new TruckDriverCardViewModel(new TruckDriverModel
            {
                Truck = _selectedTruck.TruckModel,
                Driver = obj.Driver
            });
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(Constants.SELECTED_TRUCK_DRIVER_KEY, paramObj);
            navigationParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, selectedPlanningDate);
            navigationParameters.Add(Constants.SELECTED_PLANNING_W_ASSOC_KEY, 1);
            try
            {
                var a = await navigationService.NavigateAsync("PlanningCreationPage", navigationParameters);
            }
            catch (Exception x)
            {

            }*/
        }
        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.SELECTED_TRUCK_KEY))
            {
                _selectedTruckDriverCard = parameters.GetValue<TruckDriverCardViewModel>(Constants.SELECTED_TRUCK_KEY);
                
            }
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
                    var availibleDrivers = await repository.GetAvailibleDriversForDay(selectedPlanningDate);
                    retry = false;
                    if (!availibleDrivers.Success)
                    {
                        if (IsAuthorized(availibleDrivers.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{availibleDrivers.Message}", "RIPROVA ", "CHIUDI");
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
                        _driverList = availibleDrivers.Item;
                        ConfigDriverListView();
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
            RaisePropertyChanged(nameof(PlanningDrivers));
            RaisePropertyChanged(nameof(TruckTypeColor));
            RaisePropertyChanged(nameof(TruckInternalNumber));
            RaisePropertyChanged(nameof(TruckTypeDescription));
            RaisePropertyChanged(nameof(Description));
        }

        private void ConfigDriverListView()
        {
            PlanningDrivers = new ObservableCollection<DriverCardViewModel>();
            IOrderedEnumerable<DriverModel> driverList;
            if (_viewType == 1)
            {
                driverList = _driverList.Where(x => x.DriverName.ToUpper().Contains(SearchFilter.ToUpper()) && x.CompanyDriver == -1).OrderBy(x => x.DriverName);
            }
            else
            {
                driverList = _driverList.Where(x => x.DriverName.ToUpper().Contains(SearchFilter.ToUpper()) && x.CompanyDriver == 0).OrderBy(x => x.DriverName);
            }
            foreach (var item in driverList)
            {
                PlanningDrivers.Add(new DriverCardViewModel(item));
            }
            RaisePropertyChanged(nameof(PlanningDrivers));
            RaisePropertyChanged(nameof(MessageLabel));
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

        private List<DriverModel> _driverList{ get; set; }
        private string _searchFilter = "";
        public string SearchFilter
        {
            get { return _searchFilter; }
            set { SetProperty(ref _searchFilter, value); }
        }
        private TruckDriverCardViewModel _selectedTruckDriverCard { get; set; }
        public string TruckTypeColor { get { return _selectedTruckDriverCard == null ? "" : _selectedTruckDriverCard.TruckTypeColor; } }
        public string TruckInternalNumber { get { return _selectedTruckDriverCard == null ? "" : _selectedTruckDriverCard.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return _selectedTruckDriverCard == null ? "" : _selectedTruckDriverCard.TruckTypeDescription; } }
        public string Description { get { return _selectedTruckDriverCard == null ? "" : _selectedTruckDriverCard.Description; } }

        //0-> Only Company 1-> Others
        private int _viewType { get; set; }

        public string ViewTypeDescription { get { return _viewType == 0 ? "AUTISTI AZIENDALI" : "AUTISTI PADRONCINI"; } }

        public string MessageLabel
        {
            get { return _viewType == 0 ? "AUTISTI AZIENDALI" : "AUTISTI PADRONCINI"; }
            /*
            get
            {
                if (PlanningDrivers == null)
                {
                    return "";
                }
                else if (PlanningDrivers.Count == 0)
                {
                    return "Non ci sono autisti disponibili!";
                }
                else
                {
                    return "Selezionare l'autista da associare al mezzo per il giorno lavorativo!";
                }
            }*/
        }

        public ObservableCollection<DriverCardViewModel> PlanningDrivers { get; set; }
        public ICommand SelectedDriver { get; set; }
        public ICommand ChangeView { get; set; }

    }
}
