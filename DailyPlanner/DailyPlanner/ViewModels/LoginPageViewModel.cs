using DailyPlanner.Repository;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Shiny;
using Shiny.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials.Interfaces;

namespace DailyPlanner.ViewModels
{
    public class LoginPageViewModel : NavigableViewModel, INavigationAware
    {
        private IRepository repository;
        private INavigationService navigation;
        public LoginPageViewModel(INavigationService _navigation, IRepository _repository)
            :base(_repository)
        {
            repository = _repository;
            navigation = _navigation;
            LoginCommand = new DelegateCommand(async () =>
            {
                BlockView();
                try
                {
                    var response = await _repository.LoginAsync(Username, Password);
                    if (response.Success)
                    {
                        var pushManager = Shiny.ShinyHost.Resolve<IPushManager>();
                        var result = await pushManager.RequestAccess();
                        if (result.Status == AccessState.Available)
                        {
                            // good to go

                            // you should send this to your server with a userId attached if you want to do custom work
                            var value = result.RegistrationToken;
                        }
                        await pushManager.TryClearTags();
                        await pushManager.TryAddTag(Settings.Settings.Default.LoggedUserId);
                        
                        ReleaseView();
                        NavigateOnView();
                    }
                    else
                    {
                        HandleErrorMessage(response.Message, true);
                    }
                }
                catch (Exception x)
                {
                    
                    HandleErrorMessage("Errore imprevisto durante l'autenticazione! Riprovare.\nSe il problema persiste contattare l'amministratore!", true);
                }
                ReleaseView();
            },
            () => CanNavigate && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {

            }.ObservesProperty(() => Username)
            .ObservesProperty(() => Password)
            .ObservesProperty(() => CanNavigate);
        }


        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            CanNavigate = false;
            IsBusy = true;
            if (await repository.IsUserLoggedAsync())
            {
                NavigateOnView();
            }

            CanNavigate = true;
            IsBusy = false;
        }

        private async void NavigateOnView()
        {
            if (Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.AUTISTA)
            {
                await navigation.NavigateAsync("/MainPage/NavigationPage/DriverNotificationPage");
            }
            else if (Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL)
            {
                try
                {
                    await navigation.NavigateAsync("/MainPage/NavigationPage/AdminPlanningPage");
                }
                catch(Exception x)
                {
                    string a = x.Message;
                }
            }
            else
            {
                await navigation.NavigateAsync("/MainPage/NavigationPage/PlanningPage");
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

        private void HandleErrorMessage(string errorMessage, bool visibility)
        {
            ErrorMessage = errorMessage;
            IsErrorVisible = true;
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }
        private string _errorMessage = null;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        private bool _isErrorVisible;
        public bool IsErrorVisible
        {
            get { return _isErrorVisible; }
            set { SetProperty(ref _isErrorVisible, value); }
        }
        public ICommand LoginCommand { get; private set; }
    }

}
