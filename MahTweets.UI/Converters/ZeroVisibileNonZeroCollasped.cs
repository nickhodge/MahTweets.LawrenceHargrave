using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MahTweets.UI.Converters
{
    public class ZeroVisibileNonZeroCollasped : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility rv = Visibility.Collapsed;
            int val;
            int.TryParse(value.ToString(), out val);
            if (val == 0)
                rv = Visibility.Visible;

            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}