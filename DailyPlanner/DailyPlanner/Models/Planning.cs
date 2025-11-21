using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class Planning
    {
        public int Id { get; set; }
        public TruckModel AssignedTruck { get; set; }
        public DateTime Day { get; set; }
        public DriverModel DefaultDriver { get; set; }
        public DriverModel PlannedDriver { get; set; }
        public string DefaultPlantCode { get; set; }
        public string DefaultPlantName { get; set; }
        public string PlannedPlantCode { get; set; }
        public string PlannedPlantName { get; set; }
        public string Note { get; set; }
        public DateTime PlanningStartingTime { get; set; }
        public DateTime PlanningEndingTime { get; set; }
        public int IsRental { get; set; }
        public string RentalDescription { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public DateTime StatusTime { get; set; }
        public int RowStatusCode { get; set; }
        public string RowStatusDescription { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public int NewAssoc { get; set; }
    }
}
