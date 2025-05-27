namespace Evacuation_Planning_and_Monitoring_API.Helpers
{
    public static class CalculationHelper
    {
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // distance between latitudes and longitudes
            double dLat = (Math.PI / 180) * (lat2 - lat1);
            double dLon = (Math.PI / 180) * (lon2 - lon1);

            // convert to radians
            lat1 = (Math.PI / 180) * (lat1);
            lat2 = (Math.PI / 180) * (lat2);

            // apply formulae
            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                       Math.Pow(Math.Sin(dLon / 2), 2) *
                       Math.Cos(lat1) * Math.Cos(lat2);
            double rad = 6371;
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return rad * c;

        }
        public static string CalculateETA(double distance, double speed)
        {
            if (speed <= 0)
            {
                throw new ArgumentException("Speed must be greater than zero.");
            }
            double time = (distance / speed);// time in hours, 
            int hours = (int)time;
            int minutes = (int)Math.Round((time - hours) * 60);

            if (hours > 0)
            {
                return $"{hours} hour {minutes} minute";
            }
            else
            {
                return $"{minutes} minute";
            }

        }
    }
}
