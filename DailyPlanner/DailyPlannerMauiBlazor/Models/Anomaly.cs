using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlannerMauiBlazor.Models
{
    public class Anomaly
    {
        public TruckModel AssignedTruck { get; set; }
        public DriverModel AssignedDriver { get; set; }
        public DateTime Day { get; set; }
        public string PlantCode { get; set; }
        public string NumeroBolla { get; set; }
        public int Status { get; set; }
        public string Comment { get; set; }
        public int DifferentCustomer { get; set; }
    }
}
