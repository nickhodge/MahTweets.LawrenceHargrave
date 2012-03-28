using System.Threading.Tasks;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class TwitpicProvider : IMediaProvider
    {
        // documentation here: http://dev.twitpic.com/docs/thumbnails/

        private const string _matcher =
            @"<img.*id=\x22photo-display\x22 src=\x22(?<imagesrc>http:\/\/[a-zA-Z0-9\._\'\/\-\?\=\#\&\%]*)\x22";

        private const string _twitpicapireq = @"http://twitpic.com/show/thumb/{0}";

        #region IMediaProvider Members

        public string Name
        {
            get { return "Twitpic"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://twitpic.com/");
        }

        public async Task<string> Transform(string url)
        {
            //Fetch the Twitpic page
            // TODO currently just gets the preview image rather than getting the full size image URL from the site via API. 
            // TODO I know this Warns in build due to the async qualifier on the method; but we'll add more code in here eventually
            var t = url.Split('/');
            var imgid = t[3];
            return imgid == null ? null : string.Format(_twitpicapireq, imgid);
        }

        #endregion
    }
}