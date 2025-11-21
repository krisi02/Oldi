using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DailyPlanner.ViewModels
{
    public class WorkingHoursPageViewModel : NavigableViewModel
    {
        IRepository repository;
        IPageDialogService dialogService;
        IDialogService customDialogService;
        INavigationService navigationService;
        public WorkingHoursPageViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService, IDialogService _customDialogService)
            :base(_repository)
        {
            repository = _repository;
            dialogService = _dialogService;
            SelectedDate = DateTime.Today;
            customDialogService = _customDialogService;
            navigationService = _navigationService;
            LoadData();
            BindCommands();
        }

       

        private async  void LoadData()
        {
            BlockView();
            try
            {
                bool retry = true;
                while (retry)
                {
                    var data = await repository.GetWorkingHoursList(SelectedDate);
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
                        WorkingHoursList = new ObservableCollection<WorkingHoursCardViewModel>();
                        foreach (var item in data.Item.WorkHours)
                        {
                            WorkingHoursList.Add(new WorkingHoursCardViewModel(item));
                        }
                        RaisePropertyChanged(nameof(WorkingHoursList));
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


        private void BindCommands()
        {
            SelectedWorkingHours = new DelegateCommand<WorkingHoursCardViewModel>((obj) =>
            {
                var param = new DialogParameters();
                param.Add(Constants.SELECTED_WH_CARD_KEY, obj);
                customDialogService.ShowDialog("WorkingHoursDialog", param);
            });
            RefreshData = new DelegateCommand(async () =>
            {
                LoadData();
            });
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
        public ObservableCollection<WorkingHoursCardViewModel> WorkingHoursList { get; set; }
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
                _selectedDayString = value;
                LoadData();
            }
        }
        public ICommand SelectedWorkingHours { get; set; }
        public ICommand RefreshData { get; set; }
    }
}
