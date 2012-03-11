using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MahTweets.TwitterPlugin.UI
{
    public class ParentVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = (Tweet) value;
            if (t.Parents.Count > 1)
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