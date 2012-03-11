using System;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using Windows7.Location;

namespace MahTweets.Core.Location
{
    public static class GlobalPosition
    {
        private const uint DefaultReportInterval = 0;
        private static LatLongLocationProvider _provider;
        private static bool _loadproviderthrowsexception;

        private static GeoLocation Loc { get; set; }

        public static string GetLocationUrl(GeoLocation l, int mapEngine)
        {
            switch (mapEngine)
            {
                    //bing
                case 0:
                    // Bing Maps how-to make your URL http://help.live.com/help.aspx?project=wl_local&market=en-us&querytype=topic&query=wl_local_proc_buildurl.htm
                    return String.Format("http://www.bing.com/maps/default.aspx?v=2&cp={0}~{1}&v=2&lvl=12", l.Latitude,
                                         l.Longitude);
                    //break;

                    //Google
                case 1:
                    return String.Format("http://maps.google.com/?ie=UTF8&ll={0},{1}&saddr={0},{1}&z=14", l.Latitude,
                                         l.Longitude);
                    //break;

                    //Yahoo
                case 2:
                    return String.Format("http://maps.yahoo.com/#mvt=m&lat={0}&lon={1}&q1={0}%2C{1}&zoom=14", l.Latitude,
                                         l.Longitude);
                    //break;
            }

            return null;
        }

        public static GeoLocation GetLocation()
        {
            var _applicationSettings = CompositionManager.Get<IApplicationSettingsProvider>();
            if (_applicationSettings.UseLocation)
            {
                if (Loc == null)
                {
                    var l = new GeoLocation();

                    //Try grabbing the Win7 location
                    try
                    {
                        if (_loadproviderthrowsexception) return null;

                        if (_provider == null)
                            _provider = new LatLongLocationProvider(DefaultReportInterval);

                        var position = _provider.GetReport() as LatLongLocationReport;

                        if (position == null) return null;

                        l.Latitude = position.Latitude;
                        l.Longitude = position.Longitude;

                        return l;
                    }
                    catch (Exception ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                        _loadproviderthrowsexception = true;
                        return null;
                    }
                }
            }
            return null;
        }

        public static GeoLocation SetLocation(GeoLocation newLocation)
        {
            Loc = newLocation;
            return Loc;
        }
    }
}