using DailyPlanner.Models;
using DailyPlanner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.ListViewItems
{
    public class TruckDriverViewModel
    {
        public TruckDriverModel TruckDriverObj {get; set;}

        public TruckDriverViewModel(TruckDriverModel _truckDriverModel)
        {
            TruckDriverObj = _truckDriverModel;
            _description = TruckDriverObj.Truck.TruckType + " - " + TruckDriverObj.Driver.DriverName;
        }
        private string _description;
        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(TruckDriverObj.Truck.TruckType); } }
        public string TruckInternalNumber { get { return TruckDriverObj.Truck.TruckInternalNumber; } }
        public string Description { get { return _description;} }
        
    }
}
