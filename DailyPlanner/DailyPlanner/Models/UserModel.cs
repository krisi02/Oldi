using DailyPlanner.Models.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class UserModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public RolesEnum Role { get; set; }
        public string Token { get; set; }
        public string FCMToken { get; set; }
        public string PlantCode { get; set; }
        public string PlantDescription { get; set; }

    }
}
