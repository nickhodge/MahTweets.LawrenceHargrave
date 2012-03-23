using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Settings;

namespace MahTweets.TweetProcessors.AdditionalSmarts
{
    public class TitleFromHeadTitle : IAdditonalTextSmarts
    {
        private readonly IAdditonalSmartsSettingsProvider _additonalSmartsSettingsProvider;

        public TitleFromHeadTitle() 
        {
            _additonalSmartsSettingsProvider = CompositionManager.Get<IAdditonalSmartsSettingsProvider>(); //get & cache    
        }

        #region IAdditonalTextSmarts Members

        public string Name
        {
            get { return "title"; }
        }

        public int Priority
        {
            get { return 1; }
        }

        public bool CanPageTitle(string url)
        {
            return url != null && _additonalSmartsSettingsProvider.AdditionalSmartsMapping.AsParallel().Any(u => (url.ToLower().Contains(u.Url.ToLower()) && u.ProcessType == Name));
        }

        public async Task<string> Process(String url)
        {
            var fetcher = new AsyncWebFetcher();
            var page = await fetcher.FetchAsync(url);
            if (page == null) return null;

            var titleRegex = new Regex(@"<title>(?<title>.*)<\/title>",
                                       RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            var match = titleRegex.Match(page);

            if (!match.Success) return null;
            var title = WebUtility.HtmlDecode(match.Groups["title"].Value.Replace(Environment.NewLine, ""));

            return title;
        }

        #endregion
    }
}