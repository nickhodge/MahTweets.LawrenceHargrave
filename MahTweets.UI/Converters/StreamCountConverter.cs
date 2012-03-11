using System;
using System.Globalization;
using System.Windows.Data;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.UI.Converters
{
    public class StreamCountConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}