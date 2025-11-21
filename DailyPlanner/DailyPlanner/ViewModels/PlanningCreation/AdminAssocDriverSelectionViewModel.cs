using DailyPlanner.Models;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using DailyPlanner.ViewModels.PlanningCreation;
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
    public class AdminAssocDriverSelectionViewModel : PN_BaseViewModel, IInitialize
    {
        IRepository repository;
        INavigationService navigationService;
        IPageDialogService dialogService;
        public AdminAssocDriverSelectionViewModel(IRepository _repository, INavigationService _navgationService, IPageDialogService _dialogService) :
            base(_navgationService, _repository)
        {

            navigationService = _navgationService;
            repository = _repository;
            dialogService = _dialogService;

            BindCommands();
        }

        private void BindCommands()
        {
            SelectedDriver = new DelegateCommand<DriverCardViewModel>(OnDriverSelected);
        }

        private async void OnDriverSelected(DriverCardViewModel obj)
        {
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

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.SELECTED_PLANNING_DATE_KEY))
            {
                selectedPlanningDate = parameters.GetValue<DateTime>(Constants.SELECTED_PLANNING_DATE_KEY);
            }
            if (parameters.ContainsKey(Constants.SELECTED_TRUCK_KEY))
            {
                _selectedTruck = parameters.GetValue<TruckCardViewModel>(Constants.SELECTED_TRUCK_KEY);
            }
            if (parameters.ContainsKey(Constants.LOADED_PLANNING_MATRIX_KEY))
            {
                PlanningDrivers = parameters.GetValue<ObservableCollection<DriverCardViewModel>>(Constants.LOADED_PLANNING_MATRIX_KEY);
            }
            LoadData();
        }

        private void LoadData()
        {
            RaisePropertyChanged(nameof(PlanningDrivers));
            RaisePropertyChanged(nameof(TruckTypeColor));
            RaisePropertyChanged(nameof(TruckInternalNumber));
            RaisePropertyChanged(nameof(TruckTypeDescription));
            RaisePropertyChanged(nameof(TruckDefaultPlant));
            RaisePropertyChanged(nameof(MessageLabel));
        }

        public TruckCardViewModel _selectedTruck { get; set; }

        public string TruckTypeColor { get { return _selectedTruck == null ? "" : _selectedTruck.TruckTypeColor; } }
        public string TruckInternalNumber { get { return _selectedTruck == null ? "" : _selectedTruck.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return _selectedTruck == null ? "" : _selectedTruck.TruckTypeDescription; } }
        public string TruckDefaultPlant { get { return _selectedTruck == null ? "" : _selectedTruck.TruckDefaultPlant; } }
        public string MessageLabel { get { if(PlanningDrivers == null)
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
            } 
        }

        public ObservableCollection<DriverCardViewModel> PlanningDrivers { get; set; }
        public ICommand SelectedDriver { get; set; }
    }
}
