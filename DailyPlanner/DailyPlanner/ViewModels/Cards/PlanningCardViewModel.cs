using DailyPlanner.Models;
using DailyPlanner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.Cards
{
    public class PlanningCardViewModel
    {
        public Planning planning { get; set; }
        public PlanningCardViewModel(Planning _planning)
        {
            planning = _planning;
        }

        public string DriverName { get { return planning.PlannedDriver.DriverName; } }
        public string PlanningStatusColor { get
            {
                if (planning.RowStatusCode < 4)
                {
                    return "Red";
                }
                else if (planning.RowStatusCode == 4)
                {
                    return "Green";
                }
                return "Blue";
            } }
        public string PlanningStatus { get { 
                if (planning.RowStatusCode < 4) { 
                    return "NON CONFERMATO"; 
                } 
                else if (planning.RowStatusCode == 4)
                {
                    return planning.RowStatusDescription;
                }
                return "NON PREVISTO";
            } }
        public string WorkingHoursDescription { 
            get {
                string ret = planning.IsRental != 0 ? planning.RentalDescription : "Mezzo Fisso";
                return ret + ": " + planning.PlanningStartingTime.ToString("HH:mm") + " - " + planning.PlanningEndingTime.ToString("HH:mm");
            } 
        }
        public bool IsReceivedVisibility { get
            {
                if(planning.DefaultPlantCode != planning.PlannedPlantCode)
                {
                    return true;
                }
                return false;
            } }
        public string ReceivedText
        {
            get
            {
                if (planning.DefaultPlantCode != planning.PlannedPlantCode)
                {
                    if(Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL)
                    {
                        return "Inviato a: " + planning.PlannedPlantName;
                    }
                    else
                    {
                        return "Ricevuto da: " + planning.DefaultPlantName;
                    }
                }
                return "";
            }
        }
        public bool IsNoteVisible { get
            {
                if(planning.Note != "")
                {
                    return true;
                }
                return false;
            } }
        public string Note { get { return planning.Note; } }
        public string TruckInternalNumber { get { return planning.AssignedTruck.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return planning.AssignedTruck.TruckType; } }
        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(planning.AssignedTruck.TruckType); } }
        public bool IsUpdatable { get { return planning.Day > DateTime.Today; } }
        public bool IsRevocable { get { return planning.Day > DateTime.Today; } }
    }
}
