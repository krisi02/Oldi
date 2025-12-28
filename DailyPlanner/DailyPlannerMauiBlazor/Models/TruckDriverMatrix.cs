using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlannerMauiBlazor.Models
{
    public class TruckDriverMatrix
    {
        public List<TruckDriverModel> Matrix { get; set; }
        public int Validity { get; set; }
    }
}
