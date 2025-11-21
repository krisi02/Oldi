using DailyPlanner.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.Cards
{
    public class DriverCardViewModel
    {
        public DriverModel Driver { get; set; }
        public DriverCardViewModel (DriverModel _driver)
        {
            Driver = _driver;
        }
        public string DriverName { get { return Driver.DriverName; } }
    }
}
