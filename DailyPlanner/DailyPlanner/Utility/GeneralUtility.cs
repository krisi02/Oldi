namespace DailyPlanner.Utility
{
    public class GeneralUtility
    {
        //TODO Connection Strings
        //LOCAL API CONFIG
        //public static string address = "http://10.0.2.2/TruckMoveAPI/";
        //var apiPrefix = "api/DailyPlanner/";

        //SERVER API CONFIG
        //var address = "https://truckmove1.nordestlogistica.it:5008/";
        //public static string address = "https://truckmove1.nordestlogistica.it:5008/";
        public static string address = "https://test05.jdisoftware.it";
        //public static string address ="https://frw.jdisoftware.it:9091/";
        //public static string address = "https://frw.jdisoftware.it:5005/";
        public static string apiPrefix = "api/DailyPlanner/";

        public static string GenerateTruckTypeLogoColor(string truckType)
        {
            switch (truckType)
            {
                case "ATB":
                    return "#173F5F";
                case "ATC":
                    return "#F765A3";
                case "BTP":
                    return "#3CAEA3";
                case "NAS":
                    return "#168D3D";
            }
            return "#168D3D";
        }
    }
}
