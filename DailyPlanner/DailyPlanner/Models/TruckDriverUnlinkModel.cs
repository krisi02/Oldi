using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class TruckDriverUnlinkModel
    {
        public TruckModel Truck { get; set; }
        public DateTime Day { get; set; }
    }
}
