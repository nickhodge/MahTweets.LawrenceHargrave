using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace MahTweets2.TwitterPlugin.GUITypes
{
    public class StatusColorConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 2) return null;

            if ((values[0].GetType() != typeof(bool)) || (values[1].GetType() != typeof(bool))) return null;

            bool isConnected = (bool)values[0];
            bool isPermitted = (bool)values[1];

            if (isConnected && isPermitted)
                return Colors.Green;
            else
                return Colors.Red;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class APICountColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(int))
            {
                return Colors.Transparent;
            }

            int remaining = (int)value;

            if (remaining <= 0)
            {
                return Colors.Red;
            }
            else if (remaining <= 10)
            {
                return Colors.OrangeRed;
            }
            else if (remaining <= 20)
            {
                return Colors.Orange;
            }
            else
            {
                return Colors.Green;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
