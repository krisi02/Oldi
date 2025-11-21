using DailyPlanner.Models;
using DailyPlanner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.Cards
{
     public class DriverNotificationCard
    {
        public Planning planning { get; set; }
        public DriverNotificationCard(Planning _planning)
        {
            planning = _planning;
        }
        public string Title { get { return "PIANIFICAZIONE"; } }
        public bool IsConfirmed { get { return planning == null ? false : planning.RowStatusCode == 4; } }
        public string ConfirmationMessage { get { return planning == null ? "" : $"Confermato"; } }
        public string StartingTime { get { return planning == null ? "" : "GIORNO " + planning.PlanningStartingTime.ToString("dd/MM/yyyy") + " ALLE " + planning.PlanningStartingTime.ToString("HH:mm"); } }

        public bool IsConfirmationButtonVisible { get { return planning == null ? false : planning.RowStatusCode != 4; } }
        public bool IsNoteVisible { get { return planning == null ? false : planning.Note != ""; } }
        public string StartingPlantName { get { return planning == null ? "" : planning.PlannedPlantName; } }

        public string TruckInternalNumber { get { return planning == null ? "" : planning.AssignedTruck.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return planning == null ? "" : planning.AssignedTruck.TruckType; } }
        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(planning == null ? "" : planning.AssignedTruck.TruckType); } }
    
        public string DriverNote { get { return planning == null ? "" : planning.Note; } }
    }
}
