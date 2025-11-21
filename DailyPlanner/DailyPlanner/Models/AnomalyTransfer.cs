using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class AnomalyTransfer
    {
        public int TruckCode { get; set; }
        public DateTime Day { get; set; }
        public string NewPlantCode { get; set; }
    }
}
