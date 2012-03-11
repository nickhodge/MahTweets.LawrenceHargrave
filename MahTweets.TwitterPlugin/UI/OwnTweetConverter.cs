using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using MahTweets.Core.Interfaces.Plugins;
using SelfUpdate = MahTweets.TwitterPlugin.Logic.SelfUpdate;

namespace MahTweets.TwitterPlugin.UI
{
    public class OwnTweetConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tweet = value as Tweet;
            if (tweet == null) return Visibility.Collapsed;
            IEnumerable<UpdateType> isSelf = tweet.Types.Where(t => t is SelfUpdate);
            return isSelf.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}