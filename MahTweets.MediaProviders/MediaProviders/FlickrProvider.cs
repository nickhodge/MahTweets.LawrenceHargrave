using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class FlickrEngine
    {
        // from: http://www.flickr.com/services/api/misc.urls.html
        // http://www.flickr.com/services/api/explore/flickr.photos.getInfo

        private const string _apikey = "cf532957fa7af334863226af5b0219e8";

        private const string _flickrgetphotoid =
            @"http://api.flickr.com/services/rest/?method=flickr.photos.getInfo&api_key={0}&photo_id={1}&format=rest";

        private const string _flickrimagestaticurl = @"http://farm{0}.staticflickr.com/{1}/{2}_{3}_z.jpg";
        private const string _flickrshorturl = "flic.kr/";

        private const string _flickrfindlonginshort =
            @"<link id=\x22canonicalurl\x22 rel=\x22canonical\x22 href=\x22(?<realurl>.*)\x22>";

        public async Task<string> Process(string url)
        {
            if (url == null) return null;
            string url2 = url;
            if (url.ToLower().Contains(_flickrshorturl))
            {
                var urlizer = new AsyncWebFetcher();
                string longlong = await urlizer.FetchAsync(url);
                if (longlong == null) return null;
                Match match2 = Regex.Match(longlong, _flickrfindlonginshort);
                if (!match2.Success) return null;
                url2 = match2.Groups["realurl"].Value;
            }
            string iid = FindImageID(url2);
            string getImageInfoUrl = String.Format(_flickrgetphotoid, _apikey, iid);

            var fetcher = new AsyncWebFetcher();
            string flickrdata = await fetcher.FetchAsync(getImageInfoUrl);
            if (flickrdata == null) return null;

            var xdoc = new XmlDocument();
            if (xdoc == null) return null;
            xdoc.LoadXml(flickrdata);
            XmlNode photo = xdoc.SelectSingleNode("/rsp/photo");
            if (photo == null) return null;
            return photo.Attributes == null
                       ? null
                       : String.Format(_flickrimagestaticurl, photo.Attributes["farm"].InnerText,
                                       photo.Attributes["server"].InnerText, iid, photo.Attributes["secret"].InnerText);
        }

        private string FindImageID(string url)
        {
            string[] f = url.Split('/');
            return f[5];
        }
    }

    public class FlickrProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public String Name
        {
            get { return "Flickr"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://www.flickr.com/photos/") && !url.Contains("/sets/") || url.Contains("flic.kr");
        }

        public async Task<string> Transform(string url)
        {
            var fe = new FlickrEngine();
            return await fe.Process(url);
        }

        #endregion
    }
}