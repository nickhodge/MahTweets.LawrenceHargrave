using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class PicPlzProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public String Name
        {
            get { return "PicPlz"; }
        }

        public bool Match(string url)
        {
            if (url.Contains("http://picplz.com"))
                return true;
            return false;
        }

        public async Task<string> Transform(string url)
        {
            var fetcher = new AsyncWebFetcher();
            var picplz = await fetcher.FetchAsync(url);
            if (picplz == null) return null;

            var matchPicture = Regex.Match(picplz,
                                             @"<img id=\x22mainImage\x22 src=\x22(http:\/\/[a-zA-Z0-9\._\'\/\-\?\=\#\&\%]*)\x22");

            return matchPicture.Success ? "http://" + matchPicture.Groups[1].Value : null;
        }

        #endregion
    }
}