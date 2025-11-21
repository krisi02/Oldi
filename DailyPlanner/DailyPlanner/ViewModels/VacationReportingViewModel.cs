using DailyPlanner.Models;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace DailyPlanner.ViewModels
{
    public class VacationReportingViewModel : NavigableViewModel, INavigationAware
    {
        IRepository repository;
        INavigationService navigationService;
        IPageDialogService dialogService;
        IDialogService customDialogService;
        public VacationReportingViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService, IDialogService _customDialogService)
            :base(_repository)
        {
            repository = _repository;
            navigationService = _navigationService;
            dialogService = _dialogService;
            customDialogService = _customDialogService;
            StartingDateFilter = DateTime.Today;
            EndingDateFilter = DateTime.Today.AddDays(28);
            RaisePropertyChanged(nameof(FilterString));
            BindCommands();
        }

        private void BindCommands()
        {
            AddNewVacationCommand = new DelegateCommand(async () =>
            {
                await navigationService.NavigateAsync("TruckDriverSelectionPage");
            });
            RefreshData = new DelegateCommand(() =>
            {
                LoadData();
            });
            OpenFilterDialog = new DelegateCommand(() =>
            {
                try
                {
                    var param = new DialogParameters();
                    param.Add(Constants.SELECTED_STARTING_DATE_FILTER_KEY, StartingDateFilter);
                    param.Add(Constants.SELECTED_ENDING_DATE_FILTER_KEY, EndingDateFilter);
                    customDialogService.ShowDialog("VacationReportingFilterDialog", param,r => {
                        StartingDateFilter = r.Parameters.GetValue<DateTime>(Constants.SELECTED_STARTING_DATE_FILTER_KEY);
                        EndingDateFilter = r.Parameters.GetValue<DateTime>(Constants.SELECTED_ENDING_DATE_FILTER_KEY);
                        RaisePropertyChanged(nameof(FilterString));
                    });
                }
                catch(Exception x)
                {
                    string a = x.Message;
                }
            });
            UpdateVacation = new DelegateCommand<VacationCardViewModel>(async (obj) =>
            {
                ShowUpdateVacationView(obj);
            });
            RevokeVacation = new DelegateCommand<VacationCardViewModel>(async (obj) =>
            {
                ShowRevokePlanningView(obj);
            });
        }

        private async void ShowRevokePlanningView(VacationCardViewModel obj)
        {
            bool userResponse = await dialogService.DisplayAlertAsync("ATTENZIONE", "Cliccando su \"PROCEDI\" verra revocata la segnalazione relativa al mezzo " + obj.TruckInternalNumber + " e l'autista "+ obj.DriverName +".\nQUESTA OPERAZIONE NON E' REVERSIBILE!", "PROCEDI", "CHIUDI");
            if (!userResponse) { return; }
            BlockView();
            var ret = await repository.RevokeVacation(obj.Vacation);
            if (ret.Success)
            {
                await dialogService.DisplayAlertAsync("SUCCESSO", "Operazione completata con successo!", "OK");
            }
            ReleaseView();
            LoadData();
        }

        private async void ShowUpdateVacationView(VacationCardViewModel obj)
        {
            bool userResponse = await dialogService.DisplayAlertAsync("RICHIESTA CONFERMA", "Inizia la procedura di modifica della segnalazione relativa al mezzo: " + obj.TruckInternalNumber, "PROCEDI", "CHIUDI");
            if (!userResponse) { return; }
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(Constants.SELECTED_VACATION_KEY, obj.Vacation);
            navigationParameters.Add(Constants.IS_MODIFY_KEY, true);
            try
            {
                var a = await navigationService.NavigateAsync("VacationCreation", navigationParameters);
            }
            catch (Exception x)
            {
                await dialogService.DisplayAlertAsync("ERRORE", "Errore imprevisto", "CHIUDI");
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
                    var data = await repository.GetVacationsList(StartingDateFilter,EndingDateFilter);
                    retry = false;
                    if (!data.Success)
                    {
                        if (IsAuthorized(data.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{data.Message}", "RIPROVA ", "CHIUDI");
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
                        Vacations = new ObservableCollection<VacationCardViewModel>();
                        foreach (var item in data.Item.Vacations)
                        {
                            Vacations.Add(new VacationCardViewModel(item));
                        }
                        RaisePropertyChanged(nameof(Vacations));
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
        public string FilterString { get { return StartingDateFilter.ToString("dd/MM/yyyy") + " - " + EndingDateFilter.ToString("dd/MM/yyyy"); } }
        public DateTime StartingDateFilter { get; set; }
        public DateTime EndingDateFilter { get; set; }

        public bool IsAdmin { get { return Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL; } }
        public ObservableCollection<VacationCardViewModel> Vacations { get; set; }
        public ICommand AddNewVacationCommand { get; set; }
        public ICommand RefreshData { get; set; }
        public ICommand OpenFilterDialog { get; set; }
        public ICommand UpdateVacation { get; set; }
        public ICommand RevokeVacation { get; set; }
    }
}
