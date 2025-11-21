using DailyPlanner.Models;
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
    public class AdminUnlinkTruckDriverViewModel : NavigableViewModel, INavigatedAware
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        IEventAggregator eventAggregator;
        public AdminUnlinkTruckDriverViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService, IEventAggregator _eventAggregator)
            : base(_repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            eventAggregator = _eventAggregator;
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
            if (_viewType == 0) { _viewType = 1; } else { _viewType = 0; }
            PlanningTruckDriverList = new ObservableCollection<TruckDriverCardViewModel>();
            IOrderedEnumerable<TruckDriverModel> truckDriverList;
            if (_viewType == 1)
            {
                truckDriverList = _truckDriverMatrix.Matrix.OrderBy(x => x.Truck.PlantDescription);
            }
            else
            {
                truckDriverList = _truckDriverMatrix.Matrix.OrderBy(x => x.Truck.TruckInternalNumber);
            }
            foreach (var item in truckDriverList)
            {
                if (item.Driver.DriverCode != "-1" && item.Truck.TruckCode != -1)
                {
                    if (_viewType == 0)
                    {
                        if (item.Truck.PlantDescription.Trim() == "" || item.Truck.PlantDescription.Trim() == "JOLLY")
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
            bool userResponse = await dialogService.DisplayAlertAsync("ATTENZIONE", "Sei sicuro di voler scollegare il mezzo " + obj.TruckInternalNumber + " dall'autista " + obj.Description + " per la giornata " + selectedPlanningDate.ToString("dd/MM/yyyy") + " ? \nTale operazione non è reversibile.", "PROCEDI", "CHIUDI");
            if (!userResponse) { return; }

            BlockView();
            var model = new TruckDriverUnlinkModel
            {
                Truck = obj.TruckDriverModel.Truck,
                Day = selectedPlanningDate
            };
            try
            {
                bool retry = true;
                while (retry)
                {
                    var resp = await repository.UnlinkTruckDriver(model);
                    retry = false;
                    if (!resp.Success)
                    {
                        if (IsAuthorized(resp.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{resp.Message}", "RIPROVA ", "CHIUDI");
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
                        await dialogService.DisplayAlertAsync("SUCCESSO", "Operazione completata con successo!", "OK");
                        NavigationParameters p = new NavigationParameters();
                        p.Add(Constants.EXIT_KEY, true);
                        await navigationService.GoBackAsync(p);
                    }
                }
            }
            catch (Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
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
                        PlanningTruckDriverList = new ObservableCollection<TruckDriverCardViewModel>();
                        foreach (var item in truckDriverMatrixResponse.Item.Matrix)
                        {
                            if (Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL)
                            {
                                if (item.Driver.DriverCode != "-1" && item.Truck.TruckCode != -1 && item.Truck.PlantDescription.Trim() == "")
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
            catch (Exception x)
            {
                ReleaseView();
                await dialogService.DisplayActionSheetAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
                NavigationParameters p = new NavigationParameters();
                p.Add(Constants.EXIT_KEY, true);
                await navigationService.GoBackAsync(p);
            }
        }
        private TruckDriverMatrix _truckDriverMatrix { get; set; }
        //0-> Only Jolly 1-> Others
        private int _viewType { get; set; }

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

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.SELECTED_PLANNING_DATE_KEY))
            {
                selectedPlanningDate = parameters.GetValue<DateTime>(Constants.SELECTED_PLANNING_DATE_KEY);
            }
            LoadData();
        }

        public DateTime selectedPlanningDate { get; set; }
        public ObservableCollection<TruckDriverCardViewModel> PlanningTruckDriverList { get; set; }
        public ICommand SelectedTruckDriver { get; set; }
        public bool IsAdmin { get { return Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL; } }
        public ICommand ChangeView { get; set; }
    }
}
