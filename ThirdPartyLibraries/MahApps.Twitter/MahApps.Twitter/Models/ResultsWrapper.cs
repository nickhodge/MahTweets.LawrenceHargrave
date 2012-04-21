using System.Collections.Generic;

namespace MahApps.Twitter.Models
{
    public class ResultsWrapper<T> : List<T>, ITwitterResponse
    {
    }

    public class ResultsWrapper : ITwitterResponse
    {
        public IList<SearchTweet> Results { get; set; }
    }
}