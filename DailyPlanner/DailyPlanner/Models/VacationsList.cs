using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class VacationsList
    {
        public List<VacationModel> Vacations { get; set; }
        public int Validity { get; set; }
        public VacationsList()
        {
            Vacations = new List<VacationModel>();
        }
    }
}
