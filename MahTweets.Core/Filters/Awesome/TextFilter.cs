using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Filters.Awesome
{
    public class TextFilter : IFilter
    {
        public TextFilter()
        {
            Include = true;
        }

        public string Text { get; set; }

        #region IFilter Members

        public bool Include { get; set; }

        public bool? Filter(IStatusUpdate update)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return false;

            if (string.IsNullOrWhiteSpace(update.Text))
                return false;

            return update.Text.ToLowerInvariant().Contains(Text.ToLowerInvariant());
        }

        #endregion
    }
}