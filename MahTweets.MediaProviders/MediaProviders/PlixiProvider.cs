using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class PlixiProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public string Name
        {
            get { return "Plixi"; }
        }

        public bool Match(string url)
        {
            return url.Contains("plixi.com/");
        }

        public Task<string> Transform(string url)
        {
            return Task.Run(delegate
                                  {
                                      Match match = Regex.Match(url, "http://plixi.com/p/([A-Za-z0-9]*)");
                                      if (!match.Success)
                                          return null;
                                      return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=medium&url=" + url;
                                  });
        }

        #endregion
    }
}