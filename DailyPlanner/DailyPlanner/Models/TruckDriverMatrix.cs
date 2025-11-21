using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class TruckDriverMatrix
    {
        public List<TruckDriverModel> Matrix { get; set; }
        public int Validity { get; set; }
    }
}
