using DailyPlanner.Models;
using DailyPlanner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.Cards
{
    public class AnomalyCardViewModel
    {
        public Anomaly anomaly { get; set; }
        public AnomalyCardViewModel(Anomaly _anomaly)
        {
            anomaly = _anomaly;
        }
        public string DriverName { get { return anomaly.AssignedDriver.DriverName; } }
        public string AnomalyDescription
        {
            get
            {
                return "Anomalia rilevata da rapportino";
            }
        }
        public string TruckInternalNumber { get { return anomaly.AssignedTruck.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return anomaly.AssignedTruck.TruckType; } }


        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(anomaly.AssignedTruck.TruckType); } }

    }
}
