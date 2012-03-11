using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class Smugmug : IMediaProvider
    {
        private const string _matcher =
            @"<img.*id=\x22mainImage\x22.*src=\x22(http:\/\/[a-zA-Z0-9\._\'\/\-\?\=\#\&\%]*)\x22";

        #region IMediaProvider Members

        public string Name
        {
            get { return "smugmug"; }
        }

        public bool Match(string url)
        {
            return url.Contains("smugmug.com");
        }

        public async Task<string> Transform(string url)
        {
            var _fetcher = new AsyncWebFetcher();
            string smugmug = await _fetcher.FetchAsync(url);
            if (smugmug == null) return null;
            //Find the image uri via regex
            Match match = Regex.Match(smugmug, _matcher);
            return match.Success ? match.Groups[1].Value : null;
        }

        #endregion
    }
}