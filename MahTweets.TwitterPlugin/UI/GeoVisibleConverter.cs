using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MahTweets.TwitterPlugin.UI
{
    public class GeoVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = value as Tweet;

            if (t == null)
                return Visibility.Collapsed;

            if (t.Location != null)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}