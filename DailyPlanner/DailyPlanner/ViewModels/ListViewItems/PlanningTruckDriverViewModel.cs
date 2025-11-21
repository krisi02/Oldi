using DailyPlanner.Models;
using DailyPlanner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.ListViewItems
{
    public class PlanningTruckDriverViewModel
    {
        public Planning PlanningObj { get; set; }
        public PlanningTruckDriverViewModel(Planning planning)
        {
            PlanningObj = planning;
            _description = PlanningObj.DefaultDriver.DriverName;//PlanningObj.AssignedTruck.TruckType + " - " + PlanningObj.DefaultDriver.DriverName;
        }
        private string _description;
        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(PlanningObj.AssignedTruck.TruckType); } }
        public string TruckInternalNumber { get { return PlanningObj.AssignedTruck.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return PlanningObj.AssignedTruck.TruckType; } }
        public string Description { get { return _description; } }
    }
}
