using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.Models
{
    public class PlanningRevoke
    {
        public int Id { get; set; }
        public int RevokeCode { get; set; }
        public string RevokeDescription { get; set; }
        public string UserId { get; set; }
    }
}
