using System.Linq;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Services
{
    public class ManualLongUrlRetrieverService : IManualLongUrlRetrieverService
    {
        private static readonly string[] ManualShortUrlSources =
            new[]
                {
                    "arseh.at", "feedproxy.google.com", "spr.ly", "tnw.to", "mee.bo", "spr.ly", "zite.to", "feedburner.com"
                    , "ov8.org"
                };

        #region IManualLongUrlRetrieverService Members

        public async Task<string> ExpandUrl(string shortUrl)
        {
            if (shortUrl == null) return null;
            var wc = new AsyncWebFetcher();
            string f = await wc.GetRedirectAsync(shortUrl);
#if DEBUG
            System.Console.WriteLine("{0} resolved to {1}", shortUrl, f);
#endif
            return f;
        }

        public bool IsShortUrl(string shortUrl)
        {
            if (shortUrl == null) return false;
            bool result = ManualShortUrlSources.AsParallel().Any(shortUrl.Contains);
#if DEBUG
            if (result) System.Console.WriteLine("{0} found", shortUrl);
#endif
            return result;
        }

        #endregion
    }
}