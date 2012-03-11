using System.Collections.Generic;

namespace MahTweets.Core
{
    public class SavedSearch
    {
        public string SearchTerm { get; set; }

        public int Position { get; set; }

        public IList<string> Providers { get; set; }
    }
}