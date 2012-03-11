using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using MahTweets.Core.Extensions;
using MahTweets.Core.Media;

namespace MahTweets.MediaProviders
{
    public class ImgurProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public String Name
        {
            get { return "Imgur"; }
        }

        public bool Match(string Url)
        {
            if (Url.Contains("http://imgur.com/"))
                return true;
            return false;
        }

        public MediaResult Transform(string Url)
        {
            var wc = new WebClient();
            wc.UseDefaultProxy();
            var sr = new StreamReader(wc.OpenRead(Url));
            String imgur = sr.ReadToEnd();

            Match matchPicture = Regex.Match(imgur,
                                             "<img alt=\"\" src=\"http:\\/\\/([a-zA-Z0-9\\._\\'\\/\\-\\?\\=\\#\\&\\%]*)\" original-title");

            if (matchPicture.Success)
            {
                return new MediaResult
                           {
                               MediaType = MediaType.Image,
                               Provider = this,
                               Url = "http://" + matchPicture.Groups[1].Value
                           };
            }
            return null;
        }

        #endregion
    }
}