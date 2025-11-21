using DailyPlanner.Models;
using DailyPlanner.Models.Enum;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace DailyPlanner.ViewModels.PlanningRevocation
{
    public class PlanningRevocationPageViewModel : NavigableViewModel, INavigationAware
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        public PlanningRevocationPageViewModel(INavigationService _navigationService
                                              ,IRepository _repository
                                              ,IPageDialogService _dialogService)
            :base(_repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            BindCommands();
        }

        private void BindCommands()
        {
            SavePlanningRevocation = new DelegateCommand(async () =>
            {
                bool response = await dialogService.DisplayAlertAsync("RICHIESTA CONFERMA", "Procedere con la revoca di questa pianficazione ?", "PROCEDI","CHIUDI");
                if (!response) { return; }
                BlockView();
                PlanningRevoke model = CollectDataForRevocation();
                var result = await repository.RevokePlanning(model);
                if (result.Success)
                {
                    ReleaseView(); 
                    NavigationParameters p = new NavigationParameters();
                    p.Add(Constants.EXIT_KEY, true);
                    await navigationService.GoBackAsync(p);
                }
                else
                {
                    string errorMessage = result.Message == null ? "Errore imprevisto durante il salvataggio!" : result.Message;
                    await dialogService.DisplayAlertAsync("Errore", errorMessage, "OK");
                }
                ReleaseView();
            });
        }
        private PlanningRevoke CollectDataForRevocation()
        {
            PlanningRevoke planningRevocationData = new PlanningRevoke();
            planningRevocationData.Id = _selectedPlanning.planning.Id;
            planningRevocationData.RevokeCode = SelectedCausal.Id;
            planningRevocationData.RevokeDescription = SelectedCausal.Description;
            planningRevocationData.UserId = Settings.Settings.Default.LoggedUserId;
            return planningRevocationData;
        }



        private async void LoadData(INavigationParameters parameters)
        {
            BlockView();
            try
            {
                _selectedPlanning = parameters.GetValue<PlanningCardViewModel>(Constants.SELECTED_PLANNING_KEY);
                RaisePropertyChanged(nameof(TruckTypeColor));
                RaisePropertyChanged(nameof(TruckInternalNumber));
                RaisePropertyChanged(nameof(TruckTypeDescription));
                RaisePropertyChanged(nameof(Description));
                RaisePropertyChanged(nameof(WorkingHoursDescription));
                RaisePropertyChanged(nameof(PlanningStatusDescription));
                RaisePropertyChanged(nameof(PlanningStatusColor));
                RaisePropertyChanged(nameof(SelectedDateString));

                RevocationCausalsList = new ObservableCollection<Causals>();
                var causalListModel = await repository.GetCausals(CausalTypeEnum.REVOCATION);
                if (!causalListModel.Success)
                {
                    ReleaseView();
                    await dialogService.DisplayAlertAsync("ERRORE", causalListModel.Message, "CHIUDI");
                    await navigationService.GoBackAsync(parameters: parameters);
                    return;
                }
                foreach (var c in causalListModel.Item.Causals)
                {
                    RevocationCausalsList.Add(c);
                }
                RaisePropertyChanged(nameof(RevocationCausalsList));
            }
            catch (Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("ERRORE", "Errore durante il caricamento dei dati!", "CHIUDI");
                await navigationService.GoBackAsync(parameters: parameters);
                return;
            }
            ReleaseView();
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

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.SELECTED_PLANNING_KEY))
            {
                LoadData(parameters);
            }

        }
        #region READONLY
        private PlanningCardViewModel _selectedPlanning { get; set; }
        
        public string TruckTypeColor { get { return _selectedPlanning == null ? "" : _selectedPlanning.TruckTypeColor; } }
        public string TruckInternalNumber { get { return _selectedPlanning == null ? "" : _selectedPlanning.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return _selectedPlanning == null ? "" : _selectedPlanning.TruckTypeDescription; } }
        public string Description { get { return _selectedPlanning == null ? "" : _selectedPlanning.DriverName; } }
        public string WorkingHoursDescription { get { return _selectedPlanning == null ? "" : _selectedPlanning.WorkingHoursDescription; } }
        public string PlanningStatusDescription { get { return _selectedPlanning == null ? "" : _selectedPlanning.PlanningStatus; } }
        public string PlanningStatusColor { get { return _selectedPlanning == null ? "" : _selectedPlanning.PlanningStatusColor; } }

        public string SelectedDateString { get { return _selectedPlanning == null ? "": _selectedPlanning.planning.Day.DayOfWeek.ToString() + " " + _selectedPlanning.planning.Day.ToString("dd/MM/yyyy"); } }


        #endregion
        public ObservableCollection<Causals> RevocationCausalsList { get; set; }
        
        public Causals SelectedCausal { get; set; }
        public ICommand SavePlanningRevocation { get; set; }
    }
}
