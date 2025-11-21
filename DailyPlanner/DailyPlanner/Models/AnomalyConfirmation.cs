using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class AnomalyConfirmation
    {
        public int TruckCode { get; set; }
        public DateTime Day { get; set; }
        public DateTime StartingTime { get; set; }
        public int IsRental { get; set; }
        public string UserId { get; set; }
    }
}
