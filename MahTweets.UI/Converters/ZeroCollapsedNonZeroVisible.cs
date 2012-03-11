using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MahTweets.UI.Converters
{
    //http://geekswithblogs.net/thibbard/archive/2008/12/09/wpf-ndash-hide-a-listbox-when-it-doesnrsquot-have-any.aspx
    public class ZeroCollapsedNonZeroVisible : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility rv = Visibility.Visible;
            int val = 0;
            int.TryParse(value.ToString(), out val);
            if (val == 0)
                rv = Visibility.Collapsed;

            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}