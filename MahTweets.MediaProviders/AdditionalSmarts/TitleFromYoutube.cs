using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;

namespace MahTweets.TweetProcessors.AdditionalSmarts
{
    public class TitleFroYoutube : IAdditonalTextSmarts
    {
        private static readonly string[] _pageTitleSources =
            new[]
                {
                    "youtube.com",
                    "youtu.be",
                };

        #region IAdditonalTextSmarts Members

        public string Name
        {
            get { return "TitleFromYoutube"; }
        }

        public int Priority
        {
            get { return 2; }
        }

        public bool CanPageTitle(string url)
        {
            if (url == null) return false;
            bool result = _pageTitleSources.AsParallel().Any(url.Contains);
            return result;
        }

        public async Task<string> Process(String url)
        {
            var _fetcher = new AsyncWebFetcher();
            string page = await _fetcher.FetchAsync(url);
            if (page == null) return null;

            var titleRegex = new Regex(@"<meta name=\x22title\x22 content=\x22(?<title>.*)\x22>",
                                       RegexOptions.IgnoreCase);
            Match match = titleRegex.Match(page);

            if (!match.Success) return null;
            string title = WebUtility.HtmlDecode(match.Groups["title"].Value);

            return title;
        }

        #endregion
    }
}