using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlannerMauiBlazor.Models
{
    public class WorkingHours
    {
        public TruckModel AssignedTruck { get; set; }
        public DriverModel AssignedDriver { get; set; }
        public string PlantCode { get; set; }
        public string PlantName { get; set; }
        public DateTime Day { get; set; }
        public DateTime DP_StartingTime { get; set; }
        public DateTime DP_EndingTime { get; set; }
        public DateTime TM_StartingTimeAuto { get; set; }
        public DateTime TM_EndingTimeAuto { get; set; }
        public DateTime TM_StartingTimeDriver { get; set; }
        public DateTime TM_EndingTimeDriver { get; set; }
        public DateTime LV_EndingTime { get; set; }
        public DateTime TM_BreakStartingTime { get; set; }
        public DateTime TM_BreakEndingTime { get; set; }
        public int BreakTimeDuration { get; set; }
        public string BreakTimeDurationSTR { get; set; }
        public int DailyTimeDuration { get; set; }
        public string DailyTimeDurationSTR { get; set; }
        public int IsRental { get; set; }
        public string RentalDescription { get; set; }
        public string Note { get; set; }
        public string DefaultPlantCode { get; set; }
        public string DefaultPlantName { get; set; }
        public string UserId { get; set; }
        public string DailyStatus { get; set; }
        public string DP_DailyTimeDurationSTR { get; set; }
        public string TM_AutoDailyTimeDurationSTR { get; set; }
        public string TM_DriverDailyTimeDurationSTR { get; set; }
    }
}
