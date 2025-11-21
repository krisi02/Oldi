using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class WorkingHoursList
    {
        public List<WorkingHours> WorkHours { get; set; }
        public int Validity { get; set; }
        public WorkingHoursList()
        {
            WorkHours = new List<WorkingHours>();
        }
    }
}
