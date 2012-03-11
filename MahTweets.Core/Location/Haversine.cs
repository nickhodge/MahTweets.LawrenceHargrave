using System;

namespace MahTweets.Core.Location
{
    // http://www.storm-consultancy.com/blog/development/code-snippets/the-haversine-formula-in-c-and-sql/
    // distance between any two points Geopoints

    public static class Haversine
    {
        private const double RKilometres = 6371;
        private const double RMiles = 3960;

        public static Distance CalculateDistance(GeoLocation pos1, GeoLocation pos2)
            // returns KMs between two Geopoints
        {
            return CalculateDistance(pos1, pos2, DistanceUnit.Kilometres);
        }

        public static Distance CalculateDistance(GeoLocation pos1, GeoLocation pos2, DistanceUnit unit)
            // returns KMs between two Geopoints
        {
            double r = unit == DistanceUnit.Kilometres ? RKilometres : RMiles;

            double lat = (pos2.Latitude - pos1.Latitude).ToRadians();
            var lng = (pos2.Longitude - pos1.Longitude).ToRadians();

            double h1 = Math.Sin(lat/2)*Math.Sin(lat/2) +
                        Math.Cos(ToRadians(pos1.Latitude))*Math.Cos(ToRadians(pos2.Latitude))*
                        Math.Sin(lng/2)*Math.Sin(lng/2);
            double h2 = 2*Math.Asin(Math.Min(1, Math.Sqrt(h1)));

            return new Distance(r*h2, unit);
        }

        private static double ToRadians(this double angle)
        {
            return (Math.PI/180)*angle;
        }
    }
}