using System.Text.RegularExpressions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Filters.Awesome
{
    public class HyperlinkFilter : IFilter
    {
        private static readonly Regex regexUrlPattern =
            new Regex(
                @"((http|ftp|https):\/\/)([\w\-_]+\.[\w\-_]+[\w\-\.,@!?^=%&amp;\(\):/~\+#]*[\w\-\@!?^=%&amp;/~\+\(\)#])?",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public HyperlinkFilter()
        {
            Include = true;
        }

        #region IFilter Members

        public bool Include { get; set; }

        public bool? Filter(IStatusUpdate update)
        {
            if (regexUrlPattern.Matches(update.Text).Count > 0)
                return Include;

            return null;
        }

        #endregion
    }
}