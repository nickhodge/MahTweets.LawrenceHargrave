using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class InstagramProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public String Name
        {
            get { return "Instagram"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://instagr.am");
        }

        public async Task<string> Transform(string url)
        {
            var _fetcher = new AsyncWebFetcher();
            string instagram = await _fetcher.FetchAsync(url);
            if (instagram == null) return null;

            Match matchPicture = Regex.Match(instagram,
                                             @"<img.*class=\x22photo\x22.*src=\x22(http:\/\/[a-zA-Z0-9\._\'\/\-\?\=\#\&\%]*)\x22");

            return matchPicture.Success ? matchPicture.Groups[1].Value : null;
        }

        #endregion
    }
}