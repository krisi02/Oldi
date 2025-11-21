using Akavache;
using DailyPlanner.Remote;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels;
using DailyPlanner.ViewModels.PlanningCreation;
using DailyPlanner.ViewModels.PlanningRevocation;
using DailyPlanner.Views;
using DailyPlanner.Views.AnomalyOperations;
using DailyPlanner.Views.PlanningCreation;
using DailyPlanner.Views.PlanningRevocation;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Shiny;
using System;
using System.Globalization;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace DailyPlanner
{
    public partial class App: PrismApplication
    {
        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        private void SetCultureToItalian()
        {
            CultureInfo italianEUCulture = new CultureInfo("it-IT");
            CultureInfo.DefaultThreadCurrentCulture = italianEUCulture;
        }
        protected override async void OnInitialized()
        {
            InitializeComponent();
            SetCultureToItalian();
            
            //await NavigationService.NavigateAsync("MainPage/NavigationPage/PlanningPage");
            var rsult = await NavigationService.NavigateAsync("/LoginPage");
            //await NavigationService.NavigateAsync("NavigationPage/VacationCreation");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            Akavache.Registrations.Start("Daily Planner");
            containerRegistry.RegisterInstance<Akavache.ISecureBlobCache>(Akavache.BlobCache.Secure);
            containerRegistry.RegisterInstance<Akavache.IBlobCache>(Akavache.BlobCache.UserAccount);

            var remoteAPI = new DailyPlannerAPI(GeneralUtility.address, GeneralUtility.apiPrefix);
            containerRegistry.RegisterInstance<IRemoteAPI>(remoteAPI);
            containerRegistry.RegisterSingleton<IRepository, DailyPlannerRepository>();
            containerRegistry.RegisterDialog<WorkingHoursDialog>();
            containerRegistry.RegisterDialog<VacationReportingFilterDialog>();

            containerRegistry.RegisterForNavigation<NavigationPage>();

            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<PlanningPage, PlanningPageViewModel>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<VacationReporting, VacationReportingViewModel>();
            containerRegistry.RegisterForNavigation<TruckDriverSelectionPage, TruckDriverSelectionPageViewModel>();
            containerRegistry.RegisterForNavigation<VacationCreation, VacationCreationViewModel>();
            containerRegistry.RegisterForNavigation<PlanningTruckDriverSelectionPage, PlanningTruckDriverSelectionPageViewModel>();
            
            containerRegistry.RegisterForNavigation<PlanningCreationPage, PlanningCreationPageViewModel>();
            containerRegistry.RegisterForNavigation<PlanningRevocationPage, PlanningRevocationPageViewModel>();
            containerRegistry.RegisterForNavigation<WorkingHoursPage, WorkingHoursPageViewModel>();
            containerRegistry.RegisterForNavigation<AnomaliesPage, AnomaliesPageViewModel>();
            containerRegistry.RegisterForNavigation<CompleteAnomalyPage, CompleteAnomalyPageViewModel>();
            containerRegistry.RegisterForNavigation<TransferAnomalyPage, TransferAnomalyPageViewModel>();
            containerRegistry.RegisterForNavigation<DriverNotificationPage, DriverNotificationPageViewModel>();
            //containerRegistry.RegisterForNavigation<AdminPlanningPage, AdminPlanningPageViewModel>();
            containerRegistry.RegisterForNavigation<AdminPlanningPage, AdminPlanningPageViewModel>();
            containerRegistry.RegisterForNavigation<AdminAssocTruckSelection, AdminAssocTruckSelectionViewModel>();
            containerRegistry.RegisterForNavigation<AdminAssocDriverSelection, AdminAssocDriverSelectionViewModel>();
            containerRegistry.RegisterForNavigation<AdminUnlinkTruckDriver, AdminUnlinkTruckDriverViewModel>();
            containerRegistry.RegisterForNavigation<AdminFreePlanTruckSelection, AdminFreePlanTruckSelectionViewModel>();
            containerRegistry.RegisterForNavigation<AdminFreePlanPlanning, AdminFreePlanPlanningViewModel>();
            containerRegistry.RegisterForNavigation<AdminFreePlanDriverSelection, AdminFreePlanDriverSelectionViewModel>();
        }
    }
}
