using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlannerMauiBlazor.Models
{
    public class PlanningsList
    {
        public List<Planning> Plannings { get; set; }
        public int Validity { get; set; }
        public PlanningsList()
        {
            Plannings = new List<Planning>();
        }
    }
}
