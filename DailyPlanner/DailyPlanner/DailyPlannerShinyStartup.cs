using DailyPlanner.Utility;
using Microsoft.Extensions.DependencyInjection;
using Shiny;
using Shiny.Push;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner
{
    public class DailyPlannerShinyStartup : ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services, IPlatform platform)
        {
            services.RegisterCommonServices();
            services.UseFirebaseMessaging<PushNotificationDelegate>();
        }
    }
}
