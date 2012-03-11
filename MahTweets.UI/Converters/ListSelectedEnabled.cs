using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MahTweets.UI.Converters
{
    public class ListSelectedEnabledConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((ListBox) value).SelectedIndex >= 0)
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}