using System;
using System.Globalization;
using System.Windows.Data;
using MahTweets.Core.Filters;
using MahTweets.Core.Scripting;

namespace MahTweets.UI.Converters
{
    public class ScriptFilterConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var sf = values[0] as ScriptFilter;
            var filters = values[1] as StreamModel;

            if (filters == null || sf == null)
            {
                return true;
            }

            bool found = filters.InScriptFilterActivated(sf);

            return found;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}