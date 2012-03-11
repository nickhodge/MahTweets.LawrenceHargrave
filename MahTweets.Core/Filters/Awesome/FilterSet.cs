using System.Collections.Generic;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Filters.Awesome
{
    public class FilterSet : IFilter
    {
        private readonly IList<IFilter> filters = new List<IFilter>();

        #region IFilter Members

        public bool Include { get; set; }

        public bool? Filter(IStatusUpdate update)
        {
            bool? value = null;

            foreach (IFilter filter in filters)
            {
                bool? result = filter.Filter(update);

                if (result.HasValue)
                {
                    value = result;
                    break;
                }
            }

            if (!value.HasValue)
                return null;

            return Include;
        }

        #endregion

        public void Add(IFilter filter)
        {
            filters.Add(filter);
        }
    }
}