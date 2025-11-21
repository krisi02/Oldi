using DailyPlanner.Models;
using DailyPlanner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.Cards
{
    public class TruckCardViewModel
    {
        public TruckModel TruckModel { get; set; }
        public TruckCardViewModel(TruckModel _truckModel)
        {
            TruckModel = _truckModel;
        }

        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(TruckModel.TruckType); } }
        public string TruckInternalNumber { get { return TruckModel.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return TruckModel.TruckType; } }
        public string TruckDefaultPlant { get { return TruckModel.PlantDescription; } }

    }
}
