using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class LockerzProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public String Name
        {
            get { return "Lockerz"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://lockerz.com/");
        }

        public async Task<string> Transform(string url)
        {
            var fetcher = new AsyncWebFetcher();
            var lockerz = await fetcher.FetchAsync(url);
            if (lockerz == null) return null;

            var matchPicture = Regex.Match(lockerz,
                                             @"<img id=\x22photo\x22 src=\x22(http:\/\/[a-zA-Z0-9\._\'\/\-\?\=\#\&\%]*)\x22");

            return matchPicture.Success ? matchPicture.Groups[1].Value : null;
        }

        #endregion
    }
}