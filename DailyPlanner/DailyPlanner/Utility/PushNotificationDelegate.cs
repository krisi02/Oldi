using DailyPlanner.Models.Events;
using Prism.Events;
using Shiny.Push;
using System;
using System.Threading.Tasks;

namespace DailyPlanner.Utility
{
    public class PushNotificationDelegate : IPushDelegate
    {
        public Task OnEntry(PushNotificationResponse response)
        {
            return null;
        }

        public Task OnReceived(PushNotification notification)
        {
            try
            {
                string notificationTypeValue = "";
                notification.Data.TryGetValue(Constants.NOTIFICATION_TYPE_KEY, out notificationTypeValue);
                switch (notificationTypeValue)
                {
                    case "":
                        break;
                    case Constants.NT_PLANNING_KEY:
                        var eventAggregator = (IEventAggregator)Prism.DryIoc.PrismApplication.Current.Container.Resolve(typeof(IEventAggregator));
                        eventAggregator.GetEvent<NewPlanningReceived>().Publish();
                        break;
                    case Constants.NT_ANOMALY_KEY:
                        var eventAggregator1 = (IEventAggregator)Prism.DryIoc.PrismApplication.Current.Container.Resolve(typeof(IEventAggregator));
                        eventAggregator1.GetEvent<NewAnomalyReceived>().Publish();
                        break;
                }
            }
            catch(Exception x){ }
            return null;
        }

        public Task OnTokenRefreshed(string token)
        {
            return null;
        }
    }
}
