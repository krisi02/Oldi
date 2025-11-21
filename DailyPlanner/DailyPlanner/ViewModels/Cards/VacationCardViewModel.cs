using DailyPlanner.Models;
using DailyPlanner.Utility;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.Cards
{
    public class VacationCardViewModel
    {
        public VacationModel Vacation { get; set; }
        public VacationCardViewModel(VacationModel _vacation)
        {
            Vacation = _vacation;
        }

        public string DriverName { get { return Vacation.DriverTruck.Driver.DriverName; } }
        public string CausalDescription { get { return Vacation.VacationCausal.Description; } }

        public string TruckInternalNumber { get { return Vacation.DriverTruck.Truck.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return Vacation.DriverTruck.Truck.TruckType; } }
        public string TruckTypeColor { get { return GeneralUtility.GenerateTruckTypeLogoColor(Vacation.DriverTruck.Truck.TruckType); } }

        public string TimeInterval { get { return Vacation.StartingDate.ToString("dd/MM/yyyy") + " - " + Vacation.EndDate.ToString("dd/MM/yyyy"); } }

        private bool isAdmin { get { return Settings.Settings.Default.LoggedUserRole == Models.Enum.RolesEnum.NEL; } }
        public bool IsUpdatable { get { return isAdmin && Vacation.EndDate >= DateTime.Today; } }
        public bool IsRevocable { get { return isAdmin && Vacation.EndDate >= DateTime.Today; } }

    }
}
