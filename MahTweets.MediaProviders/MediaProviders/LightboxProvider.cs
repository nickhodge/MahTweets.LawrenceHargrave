using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class LightboxProvider : IMediaProvider
    {
        private const string _matcher = @"<div.*id=\x22photo_0\x22.*img src=\x22http://(?<imgrc>(.*\.jpg))";

        #region IMediaProvider Members

        public String Name
        {
            get { return "LightboxProvider"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://lightbox.com/");
        }

        public async Task<string> Transform(string url)
        {
            var _fetcher = new AsyncWebFetcher();
            string lb = await _fetcher.FetchAsync(url);
            if (lb == null) return null;

            Match matchPicture = Regex.Match(lb, _matcher);

            return matchPicture.Success ? "http://" + matchPicture.Groups[1].Value : null;
        }

        #endregion
    }
}