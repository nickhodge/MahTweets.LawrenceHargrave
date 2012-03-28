using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class Cameraplus : IMediaProvider
    {
        #region IMediaProvider Members

        public string Name
        {
            get { return "campl.us"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://campl.us/");
        }

        public async Task<string> Transform(string url)
        {
            //Fetch the Twitpic page
            var fetcher = new AsyncWebFetcher();
            var cameraplus = await fetcher.FetchAsync(url);
            if (cameraplus == null) return null;

            //Find the image uri via regex
            var match = Regex.Match(cameraplus,
                                      @"<img*.src=\x22(?<imagesrc>http:\/\/[a-zA-Z0-9\._\'\/\-\?\=\#\&\%]*)\x22 class=\x22photo\x22");
            return match.Success ? match.Groups["imagesrc"].Value : null;
        }

        #endregion
    }
}