using DailyPlanner.Models;
using DailyPlanner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.Cards
{
    public class TruckDriverCardViewModel
    {
        public TruckDriverModel TruckDriverModel { get; set; }
        public TruckDriverCardViewModel(TruckDriverModel _truckDriverModel)
        {
            TruckDriverModel = _truckDriverModel;
        }

        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(TruckDriverModel.Truck.TruckType); } }
        public string TruckInternalNumber { get { return TruckDriverModel.Truck.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return TruckDriverModel.Truck.TruckType; } }
        public string TruckAssociatedPlant { get { return TruckDriverModel.Truck.PlantDescription; } }
        public string Description { get { return TruckDriverModel.Driver.DriverName; } }
    }
}
