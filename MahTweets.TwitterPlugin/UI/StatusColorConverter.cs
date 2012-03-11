using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MahTweets.TwitterPlugin.UI
{
    public class StatusColorConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return null;

            if ((values[0].GetType() != typeof (bool)) || (values[1].GetType() != typeof (bool))) return null;

            var isConnected = (bool) values[0];
            var isPermitted = (bool) values[1];

            if (isConnected && isPermitted)
                return Colors.Green;
            return Colors.Red;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class APICountColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (value.GetType() != typeof (int) && value.GetType() != typeof (double)))
            {
                return Colors.Transparent;
            }

            int remaining = System.Convert.ToInt32(value);

            if (remaining <= 0)
            {
                return Colors.Red;
            }
            if (remaining <= 10)
            {
                return Colors.OrangeRed;
            }
            return remaining <= 20 ? Colors.Orange : Colors.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}