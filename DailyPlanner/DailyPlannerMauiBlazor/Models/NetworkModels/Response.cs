using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlannerMauiBlazor.Models.NetworkModels
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Item { get; set; }
    }
}
