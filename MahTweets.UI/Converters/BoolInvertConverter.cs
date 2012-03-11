using System;
using System.Globalization;
using System.Windows.Data;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.UI.Converters
{
    /// <summary>
    /// Converter for inverting a boolean value
    /// </summary>
    public class BoolInvertConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Convert the boolean to its inverse
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetType">Not Used</param>
        /// <param name="parameter">Not Used</param>
        /// <param name="culture">Not Used</param>
        /// <returns>False if invalid value, inverse of value otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    var val = (bool) value;
                    return !val;
                }
                return false;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                return false;
            }
        }

        /// <summary>
        /// Convert back (not implemented)
        /// </summary>
        /// <param name="value">Not Used</param>
        /// <param name="targetType">Not Used</param>
        /// <param name="parameter">Not Used</param>
        /// <param name="culture">Not Used</param>
        /// <returns>Exception</returns>
        /// <exception cref="System.NotImplementedException">Will always throw an exception</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}