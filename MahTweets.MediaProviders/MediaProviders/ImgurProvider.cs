using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class Imgur : IMediaProvider
    {
        #region IMediaProvider Members

        public string Name
        {
            get { return "imgur"; }
        }

        public bool Match(string url)
        {
            return url.Contains("imgur.com/");
        }

        public Task<string> Transform(string url)
        {
            return Task.Run(delegate
                                  {
                                      Match match = Regex.Match(url, "gallery/(?<imgsrc>.*)");
                                      return !match.Success
                                                 ? null
                                                 : "http://i.imgur.com/" + match.Groups["imgsrc"].Value + ".jpg";
                                  });
        }

        #endregion
    }
}