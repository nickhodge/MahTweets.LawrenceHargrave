using System;
using System.Globalization;
using System.Windows.Data;

namespace MahTweets.UI.Converters
{
    public class ZeroEnabledNonZeroDisabled : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool rv = true;
            int val;
            int.TryParse(value.ToString(), out val);
            if (val == 0)
                rv = false;

            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}