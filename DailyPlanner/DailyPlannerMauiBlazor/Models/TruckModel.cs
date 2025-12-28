using DailyPlannerMauiBlazor.Models.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlannerMauiBlazor.Models
{
    public class TruckModel
    {
        public int TruckCode { get; set; }
        public string TruckInternalNumber { get; set; }
        public string TruckType { get; set; }
        public string PlantDescription { get; set; }
        public int CompanyTruck { get; set; }
    }
}
