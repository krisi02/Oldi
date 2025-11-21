using DailyPlanner.Models;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
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
    public class TransferAnomalyPageViewModel : NavigableViewModel, IInitialize
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        public TransferAnomalyPageViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService)
            :base(_repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            BindCommands();
        }

        private void BindCommands()
        {
            SelectedPlant = new DelegateCommand<Plant>(OnPlantSelected);
        }

        private async void OnPlantSelected(Plant obj)
        {
            var res = await dialogService.DisplayAlertAsync("CONFERMA", "Sei sicuro di voler inviare questo rapportino all'impianto \"" + obj.PlantDescription + "\" ?","Conferma", "Annulla");
            if(res == true)
            {
                BlockView();
                var model = CollectData(obj);
                try
                {
                    bool retry = true;
                    while (retry)
                    {
                        var resp = await repository.TransferAnomaly(model);
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
                catch(Exception x)
                {
                    ReleaseView();
                    await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
                }
            }
        }

        private AnomalyTransfer CollectData(Plant obj)
        {
            return new AnomalyTransfer
            {
                TruckCode = _anomaly.AssignedTruck.TruckCode,
                Day = _anomaly.Day,
                NewPlantCode = obj.PlantCode
            };
            throw new NotImplementedException();
        }

        private async void LoadData(INavigationParameters parameters)
        {
            BlockView();
            _anomaly = parameters.GetValue<Anomaly>(Constants.SELECTED_ANOMALY_KEY);
            try
            {
                bool retry = true;
                while (retry)
                {
                    var plantListResp = await repository.GetPlantList();
                    retry = false;
                    if (!plantListResp.Success)
                    {
                        if (IsAuthorized(plantListResp.Message))
                        {
                            ReleaseView();
                            retry = await dialogService.DisplayAlertAsync("Errore", $"{plantListResp.Message}", "RIPROVA ", "CHIUDI");
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
                        ReleaseView();
                        PlantList = new ObservableCollection<Plant>();
                        foreach (var item in plantListResp.Item.Plants)
                        {
                            PlantList.Add(item);
                        }
                        RaisePropertyChanged(nameof(PlantList));
                    }

                }
            }
            catch(Exception x)
            {
                ReleaseView();
                await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
            }
           
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

        public ObservableCollection<Plant> PlantList { get; set; }
        public ICommand SelectedPlant { get; set; }
    }
}
