using System;

namespace MahApps.Twitter.Models
{
    public class Geo : ITwitterResponse
    {
        public String Type { get; set; }

        public Double[] Coordinates { get; set; }

        public Double? Lat
        {
            get
            {
                if (Coordinates != null)
                    return Coordinates[0];
                return null;
            }
        }

        public Double? Long
        {
            get
            {
                if (Coordinates != null)
                    return Coordinates[1];
                return null;
            }
        }
    }
}