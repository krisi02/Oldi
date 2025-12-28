using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlannerMauiBlazor.Models
{
    public class AnomalyList
    {
        public List<Anomaly> Anomalies { get; set; }
        public int Validity { get; set; }
        public AnomalyList()
        {
            Anomalies = new List<Anomaly>();
        }
    }
}
