using DailyPlanner.Models;
using DailyPlanner.Models.Events;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using Prism.Commands;
using Prism.Events;
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
    public class AdminPlanningPageViewModel : NavigableViewModel, INavigatedAware
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        IEventAggregator eventAggregator;
        public AdminPlanningPageViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService, IEventAggregator _eventAggregator)
            : base(_repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            eventAggregator = _eventAggregator;
            SelectedDate = DateTime.Today.AddDays(1);
            BindEventListeners();
            BindCommands();
        }

        private void BindEventListeners()
        {
            eventAggregator.GetEvent<NewPlanningReceived>().Subscribe(LoadData);
        }


        private void BindCommands()
        {
            AddPlanning = new DelegateCommand(async () =>
            {
                if (SelectedDate <= DateTime.Today)
                {
                    await dialogService.DisplayAlertAsync("Errore", "Non è possibile pianificare per date antecedenti a quella di domani", "CHIUDI");
                    return;
                }
                NavigationParameters nParameters = new NavigationParameters();
                nParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, SelectedDate);
                var a = await navigationService.NavigateAsync("AdminFreePlanTruckSelection", nParameters);
            });
            UpdatePlanning = new DelegateCommand<PlanningCardViewModel>(async (obj) =>
            {
                ShowUpdatePlanningView(obj);
            });

            RevokePlanning = new DelegateCommand<PlanningCardViewModel>(async (obj) =>
            {
                ShowRevokePlanningView(obj);
            });
            RefreshData = new DelegateCommand(() =>
            {
                LoadData();
            });
            ReadNote = new DelegateCommand<PlanningCardViewModel>(async (obj) =>
            {
                await dialogService.DisplayAlertAsync("Nota", obj.Note, "OK");
            });
            CreateLinking = new DelegateCommand(async () =>
            {
                if (SelectedDate <= DateTime.Today)
                {
                    await dialogService.DisplayAlertAsync("Errore", "Non è possibile pianificare per date antecedenti a quella di domani", "CHIUDI");
                    return;
                }
                NavigationParameters nParameters = new NavigationParameters();
                nParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, SelectedDate);
                var a = await navigationService.NavigateAsync("AdminAssocTruckSelection", nParameters);
            });
            UnlinkAssociation = new DelegateCommand(async () =>
            {
                if (SelectedDate <= DateTime.Today)
                {
                    await dialogService.DisplayAlertAsync("Errore", "Non è possibile pianificare per date antecedenti a quella di domani", "CHIUDI");
                    return;
                }
                NavigationParameters nParameters = new NavigationParameters();
                nParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, SelectedDate);
                var a = await navigationService.NavigateAsync("AdminUnlinkTruckDriver", nParameters);
            });
        }

        private async void ShowRevokePlanningView(PlanningCardViewModel obj)
        {
            bool userResponse = await dialogService.DisplayAlertAsync("ATTENZIONE", "Contattare telefonicamente l'autista " + obj.DriverName + " prima di confermare la revoca: ", "PROCEDI", "CHIUDI");
            if (!userResponse) { return; }
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(Constants.SELECTED_PLANNING_KEY, obj);
            try
            {
                var a = await navigationService.NavigateAsync("PlanningRevocationPage", navigationParameters);
            }
            catch (Exception x)
            {
                await dialogService.DisplayAlertAsync("ERRORE", "Errore imprevisto", "CHIUDI");
            }
        }

        private async void ShowUpdatePlanningView(PlanningCardViewModel obj)
        {
            bool userResponse = await dialogService.DisplayAlertAsync("RICHIESTA CONFERMA", "Inizia la procedura di modifica della pianificazione relativa al mezzo: " + obj.TruckInternalNumber, "PROCEDI", "CHIUDI");
            if (!userResponse) { return; }
            TruckDriverModel tdModel = new TruckDriverModel()
            {
                Driver = obj.planning.PlannedDriver,
                Truck = obj.planning.AssignedTruck
            };

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(Constants.SELECTED_TRUCK_DRIVER_KEY, new TruckDriverCardViewModel(tdModel));
            navigationParameters.Add(Constants.SELECTED_PLANNING_DATE_KEY, obj.planning.Day);
            navigationParameters.Add(Constants.SELECTED_PLANNING_KEY, obj.planning);
            navigationParameters.Add(Constants.IS_MODIFY_KEY, true);
            try
            {
                var a = await navigationService.NavigateAsync("PlanningCreationPage", navigationParameters);
            }
            catch (Exception x)
            {
                await dialogService.DisplayAlertAsync("ERRORE", "Errore imprevisto", "CHIUDI");
            }
        }


        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.EXIT_KEY))
            {
                LoadData();
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
                    var planningListRepsponse = await repository.GetPlanningsListForDay(SelectedDate);
                    retry = false;
                    if (!planningListRepsponse.Success)
                    {
                        if (IsAuthorized(planningListRepsponse.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{planningListRepsponse.Message}", "RIPROVA ", "CHIUDI");
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
                        PlanningList = new ObservableCollection<PlanningCardViewModel>();
                        foreach (var item in planningListRepsponse.Item.Plannings.Where(x => x.StatusCode != 0))
                        {
                            PlanningList.Add(new PlanningCardViewModel(item));
                        }
                        RaisePropertyChanged(nameof(PlanningList));
                        ReleaseView();
                    }
                }

            }
            catch (Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
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

        public ObservableCollection<PlanningCardViewModel> PlanningList { get; set; }

        private DateTime _selectedDate { get; set; }
        public DateTime SelectedDate
        {
            get
            {
                return _selectedDate;
            }
            set
            {
                _selectedDate = value;
                RaisePropertyChanged(nameof(SelectedDate));
                SelectedDayString = value.ToString("dd/MM/yyyy");
                RaisePropertyChanged(nameof(SelectedDayString));
            }
        }
        private string _selectedDayString { get; set; }
        public string SelectedDayString
        {
            get
            {
                return _selectedDayString;
            }
            set
            {
                if (_selectedDayString != value)
                {
                    _selectedDayString = value;
                    LoadData();
                }
            }
        }


        public ICommand UnlinkAssociation { get; set; }
        public ICommand RefreshData { get; set; }
        public ICommand CreateLinking { get; set; }
        public ICommand AddPlanning { get; set; }
        public ICommand SelectDayCommand { get; set; }
        public ICommand ReadNote { get; set; }
        public ICommand UpdatePlanning { get; set; }
        public ICommand RevokePlanning { get; set; }
    }
}
