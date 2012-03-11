using System;

namespace MahApps.Twitter.Models
{
    public class SavedSearch : ITwitterResponse
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String Query { get; set; }
    }
}