using System;
using Windows7.Location;

namespace MahTweets.Core.Global
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Location() { }
        public Location(double Latitude, double Longitude)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
        }
    }
    public static class GlobalPosition
    {
        private static LatLongLocationProvider _provider;
        private static bool _loadproviderthrowsexception = false;

        private const uint DEFAULT_REPORT_INTERVAL = 0;

        private static Location loc { get; set; }

        public static String GetLocationUrl(Location l, int mapEngine)
        {
            switch (mapEngine)
            {
                    //bing
                case 0 :
                    // Bing Maps how-to make your URL http://help.live.com/help.aspx?project=wl_local&market=en-us&querytype=topic&query=wl_local_proc_buildurl.htm
                    return String.Format("http://www.bing.com/maps/default.aspx?v=2&cp={0}~{1}&v=2&lvl=12", l.Latitude, l.Longitude);
                    //break;
                
                    //Google
                case 1:
                    return String.Format("http://maps.google.com/?ie=UTF8&ll={0},{1}&saddr={0},{1}&z=14", l.Latitude, l.Longitude);
                    //break;
                
                    //Yahoo
                case 2:
                    return String.Format("http://maps.yahoo.com/#mvt=m&lat={0}&lon={1}&q1={0}%2C{1}&zoom=14", l.Latitude, l.Longitude);
                    //break;
            }

            return null;
        }

        public static Location GetLocation()
        {
            if (loc == null)
            {
                Location l = new Location();

                //Try grabbing the Win7 location
                try
                {
                    if (_loadproviderthrowsexception)
                    {
                        return null;
                    }

                    if (_provider == null)
                        _provider = new LatLongLocationProvider(DEFAULT_REPORT_INTERVAL);

                    LatLongLocationReport position = _provider.GetReport() as LatLongLocationReport;

                    l.Latitude = position.Latitude;
                    l.Longitude = position.Longitude;

                    return l;
                }
                catch (Exception ex)
                {
                    BlackBoxRecorder.LogHandledException(ex);

                    //TODO: Try grabbing the mahtweets location
                    _loadproviderthrowsexception = true;
                    return null;

                }
            }
            else
                return loc;

        }

        public static Location SetLocation(Location newLocation)
        {
            if (loc == null)
                loc = new Location();
            loc = newLocation;
            return loc;
        }

    }
}


