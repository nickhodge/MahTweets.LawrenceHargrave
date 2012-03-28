using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class MobyProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public string Name
        {
            get { return "Moby"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://moby.to/");
        }

        public Task<string> Transform(string url)
        {
            return Task.Run(delegate
                                  {
                                      var match = Regex.Match(url, "moby.to/([A-Za-z0-9]*)");
                                      return !match.Success ? null : url + ":medium";
                                  });
        }

        #endregion
    }
}