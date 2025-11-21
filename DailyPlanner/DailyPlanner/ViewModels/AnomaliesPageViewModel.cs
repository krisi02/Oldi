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
    public class AnomaliesPageViewModel : NavigableViewModel, INavigatedAware
    {
        INavigationService navigationService;
        IRepository repository;
        IEventAggregator eventAggregator;
        IPageDialogService dialogService;
        public AnomaliesPageViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService, IEventAggregator _eventAggregator)
            :base(_repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            eventAggregator = _eventAggregator;
            BindEventListeneres();
            BindCommands();
        }

        private void BindEventListeneres()
        {
            eventAggregator.GetEvent<NewAnomalyReceived>().Subscribe(LoadData);
        }

        private void BindCommands()
        {
            CompleteAnomaly = new DelegateCommand<AnomalyCardViewModel>(OnAnomalyCompleteRequest);
            RefreshData = new DelegateCommand(async () =>
            {
                LoadData();
            });
            ViewDetails = new DelegateCommand<AnomalyCardViewModel>(async (obj) =>
            {
                await dialogService.DisplayAlertAsync("DETTAGLIO ANOMALIA "+obj.TruckInternalNumber, obj.anomaly.Comment, "OK");
            });
        }


        private async void OnAnomalyCompleteRequest(AnomalyCardViewModel obj)
        {
            string[] buttons = new string[2];
            if(Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.IMPIANTISTA)
            {
                buttons[0] = "Inserimento dati";
                buttons[1] = "Trasferisci";
            }
            else
            {
                buttons[0] = "Trasferisci";
            }

            var res = await dialogService.DisplayActionSheetAsync("Operazione", "Annulla", "", buttons);
            NavigationParameters nParameters = new NavigationParameters();
            nParameters.Add(Constants.SELECTED_ANOMALY_KEY, obj.anomaly);
            if (res == "Inserimento dati")
            {
                var a = await navigationService.NavigateAsync("CompleteAnomalyPage", nParameters);
            }
            else if (res == "Trasferisci")
            {
                var a = await navigationService.NavigateAsync("TransferAnomalyPage", nParameters);
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
                    var anomaliesList = await repository.GetAnomalies();
                    retry = false;
                    if (!anomaliesList.Success)
                    {
                        if (IsAuthorized(anomaliesList.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{anomaliesList.Message}", "RIPROVA ", "CHIUDI");
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
                        AnomalyList = new ObservableCollection<AnomalyCardViewModel>();
                        foreach (var item in anomaliesList.Item.Anomalies)
                        {
                            AnomalyList.Add(new AnomalyCardViewModel(item));
                        }
                        RaisePropertyChanged(nameof(AnomalyList));
                        ReleaseView();
                    }
                }
            }
            catch(Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
            }
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
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

        public ObservableCollection<AnomalyCardViewModel> AnomalyList { get; set; }

        public ICommand CompleteAnomaly { get; set; }
        public ICommand RefreshData { get; set; }
        public ICommand ViewDetails { get; set; }
    }
}
