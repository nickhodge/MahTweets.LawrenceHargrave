using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MahTweets.TwitterPlugin.Views
{
    public class MoreThanOneTweetWorthConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = ((int) value);

            if (count > 140)
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