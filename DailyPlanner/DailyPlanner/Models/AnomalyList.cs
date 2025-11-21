using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
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
