namespace MahTweets.Core.Location
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Location() { }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}