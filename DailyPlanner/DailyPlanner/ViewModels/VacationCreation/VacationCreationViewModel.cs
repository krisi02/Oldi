using DailyPlanner.Models;
using DailyPlanner.Models.NetworkModels;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using DailyPlanner.ViewModels.ListViewItems;
using Prism.Commands;
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
    public class VacationCreationViewModel : NavigableViewModel, IInitialize
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        public VacationCreationViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService)
            :base(_repository)
        {
            repository = _repository;
            navigationService = _navigationService;
            dialogService = _dialogService;
            BindCommands();
            
        }

        private async void LoadData()
        {
            BlockView();
            TimeZoneInfo timeZone = TimeZoneInfo.Local;
            var utcOffset = timeZone.GetUtcOffset(DateTime.Today);
            StartingDate = DateTime.Today.Add(utcOffset);
            EndingDate = DateTime.Today.AddDays(1).Add(utcOffset);

            RaisePropertyChanged(nameof(StartingDate));
            RaisePropertyChanged(nameof(EndingDate));
            try
            {
                bool retry = true;
                while (retry)
                {
                    var causalList = await repository.GetCausals(Models.Enum.CausalTypeEnum.VACATION);
                    retry = false;
                    if (!causalList.Success)
                    {
                        if(IsAuthorized(causalList.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{causalList.Message}", "RIPROVA ", "CHIUDI");
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
                        VacationCausalsList = new ObservableCollection<Causals>();
                        foreach (var c in causalList.Item.Causals)
                        {
                            VacationCausalsList.Add(c);
                        }
                        RaisePropertyChanged(nameof(VacationCausalsList));
                        ReleaseView();
                    }
                }
                if (_isModify)
                {
                    StartingDate = _selectedVacation.StartingDate;
                    RaisePropertyChanged(nameof(StartingDate));
                    EndingDate = _selectedVacation.EndDate;
                    RaisePropertyChanged(nameof(EndingDate));
                    RaisePropertyChanged(nameof(IsStartingDateEnabled));
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
            SaveNewVacation = new DelegateCommand(async () =>
            {
                bool userResponse = await dialogService.DisplayAlertAsync("RICHIESTA CONFERMA", "Procedere con il salvataggio dei dati ?", "PROCEDI", "CHIUDI");
                if (!userResponse) { return; }
                if(EndingDate < StartingDate)
                {
                    await dialogService.DisplayAlertAsync("ATTENZIONE", "La data di fine dovrebbe essere successiva alla data d'inizio!", "OK");
                    return;
                }
                BlockView();
                VacationModel model = CollectData();
                var ret = new Response<bool>();
                if (!_isModify)
                {
                     ret = await repository.CreateVacation(model);
                }
                else
                {
                     ret = await repository.UpdateVacation(model);
                }
                ReleaseView();
                if (ret.Success)
                {
                    NavigationParameters p = new NavigationParameters();
                    p.Add(Constants.EXIT_KEY, true);
                    await navigationService.GoBackAsync(p);
                }
                else
                {
                    string errorMessage = ret.Message == null ? "Errore imprevisto durante il salvataggio!" : ret.Message;
                    await dialogService.DisplayAlertAsync("Errore", errorMessage, "OK");
                }
            },
            () => CanNavigate && SelectedVacationCausal != null)
            .ObservesProperty(() => EndingDate)
            .ObservesProperty(() => StartingDate)
            .ObservesProperty(() => SelectedVacationCausal)
            .ObservesProperty(() => CanNavigate);
        
    }

        private VacationModel CollectData()
        {
            return new VacationModel
            {
                DriverTruck = _selectedTruckDriver.TruckDriverModel,
                VacationCausal = _selectedVacationCausal,
                StartingDate = StartingDate,
                EndDate = EndingDate,
                UserId = Settings.Settings.Default.LoggedUserId
            };
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

        public void Initialize(INavigationParameters parameters)
        {
            _isModify = parameters.GetValue<bool>(Constants.IS_MODIFY_KEY);
            if (_isModify)
            {
                _selectedVacation = parameters.GetValue<VacationModel>(Constants.SELECTED_VACATION_KEY);
                _selectedTruckDriver = new TruckDriverCardViewModel(_selectedVacation.DriverTruck);
            }
            else
            {
                _selectedTruckDriver = (TruckDriverCardViewModel)parameters[Constants.SELECTED_VC_TRUCK_DRIVER_KEY];
            }
            RaisePropertyChanged(nameof(PageTitle));
            RaisePropertyChanged(nameof(TruckTypeColor));
            RaisePropertyChanged(nameof(TruckInternalNumber));
            RaisePropertyChanged(nameof(TruckTypeDescription));
            RaisePropertyChanged(nameof(Description));
           
            LoadData();
        }


        private VacationModel _selectedVacation { get; set; }
        private bool _isModify { get; set; }

        public ICommand SaveNewVacation { get; private set; }
        public ObservableCollection<Causals> VacationCausalsList { get; set; }

        private Causals _selectedVacationCausal;
        public Causals SelectedVacationCausal { get { return _selectedVacationCausal; } set { SetProperty(ref _selectedVacationCausal, value); } }
        
        public Boolean IsStartingDateEnabled { get { return !_isModify; } }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }

        public string PageTitle { get { return _isModify ? "MODIFICA SEGNALAZIONE" : "NUOVA SEGNALAZIONE"; } }
        
        private TruckDriverCardViewModel _selectedTruckDriver { get; set; }
        public string TruckTypeColor { get { return _selectedTruckDriver == null ? "": _selectedTruckDriver.TruckTypeColor; } }
        public string TruckInternalNumber { get { return _selectedTruckDriver == null ? "" : _selectedTruckDriver.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return _selectedTruckDriver == null ? "" : _selectedTruckDriver.TruckTypeDescription; } }
        public string Description { get { return _selectedTruckDriver == null ? "" : _selectedTruckDriver.Description;  } }
    }
}
