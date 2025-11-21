using DailyPlanner.Models;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DailyPlanner.ViewModels
{
    public class CompleteAnomalyPageViewModel : NavigableViewModel, IInitialize
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        public CompleteAnomalyPageViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService)
            :base(_repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            BindCommands();
        }

        private void BindCommands()
        {
            CompleteAnomaly = new DelegateCommand(async () =>
            {
                bool userResponse = await dialogService.DisplayAlertAsync("RICHIESTA CONFERMA", "Procedere con il salvataggio dei dati ?", "PROCEDI", "CHIUDI");
                if (!userResponse) { return; }
                BlockView();
                var model = CollectData();
                try
                {
                    var resp = await repository.CompleteAnomaly(model);
                    if (!resp.Success)
                    {
                        if (IsAuthorized(resp.Message))
                        {
                            ReleaseView();
                            await dialogService.DisplayAlertAsync("Errore", $"{resp.Message}", "CHIUDI");
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
                catch(Exception x)
                {
                    ReleaseView();
                    await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
                }
            });
        }

        private AnomalyConfirmation CollectData()
        {
            return new AnomalyConfirmation
            {
                Day = _anomaly.Day,
                StartingTime = _anomaly.Day.Add(StartingTime),
                TruckCode = _anomaly.AssignedTruck.TruckCode,
                IsRental = IsRental ? 1 : 0,
                UserId = Settings.Settings.Default.LoggedUserId
            };
        }

        private void LoadData(INavigationParameters parameters)
        {
            _anomaly = parameters.GetValue<Anomaly>(Constants.SELECTED_ANOMALY_KEY);
            RaisePropertyChanged(nameof(SelectedDateString));
            RaisePropertyChanged(nameof(TruckTypeColor));
            RaisePropertyChanged(nameof(TruckInternalNumber));
            _description = _anomaly.AssignedTruck.TruckType + " - " + _anomaly.AssignedDriver.DriverName;
            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(TruckTypeDescription));
            StartingTime = DateTime.Now.TimeOfDay;// selectedPlanning.PlanningObj.PlanningStartingTime;
            IsRental = false;
        }

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.SELECTED_ANOMALY_KEY))
            {
                LoadData(parameters);
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

        private Anomaly _anomaly { get; set; }
        public ICommand CompleteAnomaly { get; set; }

        #region READONLY
        private string _description { get; set; }
        public string SelectedDateString { get { return _anomaly == null ? "" : _anomaly.Day.DayOfWeek.ToString() + " " + _anomaly.Day.ToString("dd/MM/yyyy"); } }
        public string TruckTypeColor { get { return _anomaly == null ? "" : GeneralUtility.GenerateTruckTypeLogoColor(_anomaly.AssignedTruck.TruckType); } }
        public string TruckTypeDescription { get { return _anomaly == null ? "" : _anomaly.AssignedTruck.TruckType; } }
        public string TruckInternalNumber { get { return _anomaly == null ? "" : _anomaly.AssignedTruck.TruckInternalNumber; } }
        public string Description { get { return _description; } }
        #endregion
        public TimeSpan StartingTime { get; set; }
        public bool IsRental { get; set; }

    }
}
