using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class VacationModel
    {
        public TruckDriverModel DriverTruck {get; set;}
        public Causals VacationCausal { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
