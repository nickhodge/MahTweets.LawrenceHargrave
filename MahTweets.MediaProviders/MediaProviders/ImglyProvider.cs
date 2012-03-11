using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class Imgly : IMediaProvider
    {
        private const string _matcher =
            @"<img.*id=\x22the-image\x22.*src=\x22(http:\/\/[a-zA-Z0-9\._\'\/\-\?\=\#\&\%]*)\x22";

        #region IMediaProvider Members

        public string Name
        {
            get { return "img.ly"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://img.ly/");
        }

        public async Task<string> Transform(string url)
        {
            var _fetcher = new AsyncWebFetcher();
            string imgly = await _fetcher.FetchAsync(url);
            if (imgly == null) return null;
            Match match = Regex.Match(imgly, _matcher);
            return match.Success ? match.Groups[1].Value : null;
        }

        #endregion
    }
}