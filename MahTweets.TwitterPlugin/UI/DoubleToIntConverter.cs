using System;
using System.Globalization;
using System.Windows.Data;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.TwitterPlugin.UI
{
    public class DoubleToIntConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertToInteger(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertToInteger(value);
        }

        #endregion

        private static object ConvertToInteger(object value)
        {
            try
            {
                return System.Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            return 0;
        }
    }


    public class ListCalculator : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return 0;

            int firstNumber = System.Convert.ToInt32(values[0]);
            int myLists = System.Convert.ToInt32(values[1]);
            int otherLists = System.Convert.ToInt32(values[2]);


            return System.Convert.ToString(firstNumber*(myLists + otherLists));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}