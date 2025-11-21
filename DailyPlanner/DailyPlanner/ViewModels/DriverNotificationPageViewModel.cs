using DailyPlanner.Models;
using DailyPlanner.Models.Events;
using DailyPlanner.Repository;
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

namespace DailyPlanner.ViewModels
{
    public class DriverNotificationPageViewModel : NavigableViewModel
    {
        INavigationService navigationService;
        IRepository repository;
        IEventAggregator eventAggregator;
        IPageDialogService dialogService;
        public DriverNotificationPageViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService, IEventAggregator _eventAggregator)
            :base(_repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            eventAggregator = _eventAggregator;
            LoadData();
            BindCommands();
            BindEventListeners();
        }

        private void BindEventListeners()
        {
            eventAggregator.GetEvent<NewPlanningReceived>().Subscribe(LoadData);
        }

        private void BindCommands()
        {
            ReadNote = new DelegateCommand<DriverNotificationCard>(async (obj) =>
            {
                await dialogService.DisplayAlertAsync("Nota", obj.planning.Note, "OK");
            });
            ConfirmPlanning = new DelegateCommand<DriverNotificationCard>(async (obj) =>
            {
                if (await dialogService.DisplayAlertAsync("CONFERMA", "Sei sicuro di voler confermare questa pianificazione?", "SI", "ANNULLA"))
                {
                    try
                    {
                        bool retry = true;
                        while (retry)
                        {
                            var result = await repository.DriverPlanningConfirmation(new DriverPlanningConfirmation { PlanningId = obj.planning.Id, UserId = Settings.Settings.Default.LoggedUserId });
                            retry = false;
                            if (!result.Success)
                            {
                                if (IsAuthorized(result.Message))
                                {
                                    ReleaseView();
                                    retry = await dialogService.DisplayAlertAsync("Errore", $"{result.Message}", "RIPROVA ", "CHIUDI");
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
                                LoadData();
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
                    }
                }
            });
            RefreshData = new DelegateCommand(() =>
            {
                LoadData();
            });
        }

        private async void LoadData()
        {
            BlockView();
            try
            {
                bool retry = true;
                while (retry)
                {
                    var driverNotifications = await repository.GetDriverNotifications();
                    retry = false;
                    if (!driverNotifications.Success)
                    {
                        if (IsAuthorized(driverNotifications.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{driverNotifications.Message}", "RIPROVA ", "CHIUDI");
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
                        NotificationList = new ObservableCollection<DriverNotificationCard>();
                        foreach (var item in driverNotifications.Item.Plannings)
                        {
                            NotificationList.Add(new DriverNotificationCard(item));
                        }
                        RaisePropertyChanged(nameof(NotificationList));
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

        public ObservableCollection<DriverNotificationCard> NotificationList { get; set; }
        public ICommand RefreshData { get; set; }
        public ICommand ReadNote { get; set; }
        public ICommand ConfirmPlanning { get; set; }


    }
}
