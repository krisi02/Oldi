using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Shiny;

namespace DailyPlanner.Droid
{
    [Application(
        Theme = "@style/MainTheme"
        )]
    public partial class MainApplication : Application
    {
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            //ShinyHost.Init(new AndroidPlatform(this));
            
            this.ShinyOnCreate(new DailyPlannerShinyStartup());
            Xamarin.Essentials.Platform.Init(this);
        }
    }
}
