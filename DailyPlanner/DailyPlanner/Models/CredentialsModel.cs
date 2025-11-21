using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class CredentialsModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FCMToken { get; set; }
    }
}
