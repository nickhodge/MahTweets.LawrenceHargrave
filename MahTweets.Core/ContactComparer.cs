using System.Collections.Generic;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core
{
    public class ContactComparer : IComparer<IContact>
    {
        #region IComparer<IContact> Members

        public int Compare(IContact x, IContact y)
        {
            if (x == null || y == null)
                return -1;

            if (string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrWhiteSpace(y.Name))
                return 1;

            bool matches = x.Name.Matches(y.Name);

            return matches ? 0 : 9;
        }

        #endregion
    }
}