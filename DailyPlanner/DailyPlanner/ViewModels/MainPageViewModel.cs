using DailyPlanner.Models;
using DailyPlanner.Models.Enum;
using DailyPlanner.Repository;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Shiny;
using Shiny.Push;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DailyPlanner.ViewModels
{
    public class MainPageViewModel : NavigableViewModel
    {
        INavigationService navigationService;
        IRepository repository;
        IPageDialogService dialogService;
        public ICommand SelectedMenuItem { get; set; }

        public MainPageViewModel(INavigationService _navigationService, IRepository _repository, IPageDialogService _dialogService)
            :base(_repository)
        {
            navigationService = _navigationService;
            repository = _repository;
            dialogService = _dialogService;
            BindCommands();
            CreateMenu();
        }

        private void BindCommands()
        {
            SelectedMenuItem = new DelegateCommand<SideMenuItem>(async(tappedItem) => {
                var a = await navigationService.NavigateAsync("MainPage/NavigationPage/" + tappedItem.ViewName);
            });
            LogoutCommand = new DelegateCommand(async ()=>
            {
                try
                {
                    bool retry = true;
                    while (retry)
                    {
                        BlockView();
                        var ret = await repository.LogoutAsync(Settings.Settings.Default.LoggedUserId);
                        retry = false;
                        if (!ret.Success)
                        {
                            if (IsAuthorized(ret.Message))
                            {
                                ReleaseView();
                                retry = await dialogService.DisplayAlertAsync("Errore", $"{ret.Message}", "RIPROVA ", "CHIUDI");
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
                            var pushManager = Shiny.ShinyHost.Resolve<IPushManager>();
                            var result = await pushManager.RequestAccess();
                            if (result.Status == AccessState.Available)
                            {
                                // good to go

                                // you should send this to your server with a userId attached if you want to do custom work
                                var value = result.RegistrationToken;
                            }
                            await pushManager.TryClearTags();
                            await navigationService.NavigateAsync("/LoginPage");
                        }
                    }
                }
                catch (Exception x)
                {
                    ReleaseView();
                    await dialogService.DisplayAlertAsync("Errore", "Errore sconosciuto durante il caricamento dei dati!\r\nContattare l'amministratore ", "CHIUDI");
                }

            });
        }

        private async void CreateMenu()
        {
            var loggedUser = await repository.GetUser();
            _userFullName = loggedUser.FullName;
            _plantName = loggedUser.PlantDescription;
            RaisePropertyChanged(nameof(UserFullName));
            RaisePropertyChanged(nameof(PlantName));
            SideBarMenu = new ObservableCollection<SideMenuItem>();
            
            if (loggedUser.Role == RolesEnum.IMPIANTISTA)
            {
                //clipboard-list
                SideBarMenu.Add(new SideMenuItem { Id = (int)MenuItemTypes.Pianificazioni, Title = "Pianificazioni", Icon = FontAwesome.FontAwesomeIcons.Clock, ViewName = "PlanningPage" });
                SideBarMenu.Add(new SideMenuItem { Id = (int)MenuItemTypes.AttivitaGiornaliere, Title = "Attivita Giornaliere", Icon = FontAwesome.FontAwesomeIcons.ListAlt, ViewName = "WorkingHoursPage" });
                SideBarMenu.Add(new SideMenuItem { Id = (int)MenuItemTypes.Anomalie, Title = "Anomalie", Icon = FontAwesome.FontAwesomeIcons.ExclamationTriangle, ViewName = "AnomaliesPage" });
                SideBarMenu.Add(new SideMenuItem { Id = (int)MenuItemTypes.Segnalazioni, Title = "Segnalazioni", Icon = FontAwesome.FontAwesomeIcons.Bullhorn, ViewName = "VacationReporting" });

            }
            else if (loggedUser.Role == RolesEnum.AUTISTA)
            {
                SideBarMenu.Add(new SideMenuItem { Id = (int)MenuItemTypes.Pianificazioni, Title = "Notifiche", Icon = FontAwesome.FontAwesomeIcons.Bell, ViewName = "DriverNotification" });
            }
            else if (loggedUser.Role == RolesEnum.NEL)
            {
                SideBarMenu.Add(new SideMenuItem { Id = (int)MenuItemTypes.Pianificazioni, Title = "Pianificazioni", Icon = FontAwesome.FontAwesomeIcons.Clock, ViewName = "AdminPlanningPage" });
                SideBarMenu.Add(new SideMenuItem { Id = (int)MenuItemTypes.Anomalie, Title = "Anomalie", Icon = FontAwesome.FontAwesomeIcons.ExclamationTriangle, ViewName = "AnomaliesPage" });
                SideBarMenu.Add(new SideMenuItem { Id = (int)MenuItemTypes.Segnalazioni, Title = "Segnalazioni", Icon = FontAwesome.FontAwesomeIcons.Bullhorn, ViewName = "VacationReporting" });
            }
            RaisePropertyChanged(nameof(SideBarMenu));

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
        public ObservableCollection<SideMenuItem> SideBarMenu { get; set; }
        public string _userFullName { get; set; }
        public string _plantName { get; set; }
        public string UserFullName { get { return _userFullName; }  }
        public string PlantName { get { return "IMPIANTO: "+_plantName; } }

        public ICommand LogoutCommand { get; set; }
    }
    
}
