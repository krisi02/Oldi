using DailyPlanner.Models;
using DailyPlanner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.Cards
{

    public class WorkingHoursCardViewModel
    {
        public WorkingHours workingHours { get; set; }
        public WorkingHoursCardViewModel(WorkingHours _workingHours)
        {
            workingHours = _workingHours;
        }
        public string DriverName { get { return workingHours.AssignedDriver.DriverName; } }
        public string WorkingHoursDescription
        {
            get
            {
                string ret = workingHours.IsRental != 0 ? workingHours.RentalDescription : "MEZZO FISSO";
                ret += ": DALLE " + workingHours.DP_StartingTime.ToString("HH:mm");
                if (workingHours.DailyStatus.Split('-')[0] != "O")
                {
                    ret += " ALLE " + workingHours.LV_EndingTime.ToString("HH:mm");
                }
                return ret;

            }
        }
        public string BreakDescription
        {
            get
            {
                return workingHours.BreakTimeDuration != 0 ? "PAUSA RILEVATA: " + workingHours.BreakTimeDurationSTR : "NESSUNA PAUSA RILEVATA";
            }
        }
        public string DailyStatusColor { get {
                var a = workingHours.DailyStatus.Split('-');
                return a[a.Length - 1]; } }
        public string TruckInternalNumber { get { return workingHours.AssignedTruck.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return workingHours.AssignedTruck.TruckType; } }
        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(workingHours.AssignedTruck.TruckType); } }


    }
}
