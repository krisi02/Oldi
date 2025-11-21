using Foundation;
using Prism;
using Prism.Ioc;
using Shiny;
using System;
using UIKit;

namespace DailyPlanner.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {  
        // special method called by shiny if exists
        partial void OnPreFinishedLaunching(UIApplication app, NSDictionary options);
        // special method called by shiny if exists
        partial void OnPostFinishedLaunching(UIApplication app, NSDictionary options);

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            //this.OnPreFinishedLaunching(app, options);
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App(new iOSInitializer()));
            this.ShinyFinishedLaunching(new DailyPlannerShinyStartup());
            return base.FinishedLaunching(app, options);
        }
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
       => this.ShinyRegisteredForRemoteNotifications(deviceToken);

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
            => this.ShinyFailedToRegisterForRemoteNotifications(error);

        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
            => this.ShinyDidReceiveRemoteNotification(userInfo, completionHandler);
        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
      => this.ShinyPerformFetch(completionHandler);

        /*
        THIS METHOD IS NEEEDED IF YOU ARE USING SHINY.NET.HTTP
        */
        public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
            => this.ShinyHandleEventsForBackgroundUrl(sessionIdentifier, completionHandler);


    }



    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }
}
