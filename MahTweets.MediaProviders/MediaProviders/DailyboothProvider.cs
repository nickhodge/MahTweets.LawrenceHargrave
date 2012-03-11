using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class DailyboothProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public String Name
        {
            get { return "Dailybooth"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://dailybooth.com/");
        }

        public async Task<string> Transform(string url)
        {
            var _fetcher = new AsyncWebFetcher();
            var dbooth = await _fetcher.FetchAsync(url);

            var matchPicture = Regex.Match(dbooth, @"<img src=\x22(http:\/\/[a-zA-Z0-9\._\'\/\-\?\=\#\&\%]*)\x22");

            return matchPicture.Success ? "http://" + matchPicture.Groups[1].Value : null;
        }

        #endregion
    }
}