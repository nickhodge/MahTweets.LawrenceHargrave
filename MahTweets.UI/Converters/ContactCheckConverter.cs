using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MahTweets.Core;
using MahTweets.Core.Filters;

namespace MahTweets.UI.Converters
{
    /// <summary>
    /// Converter to return a boolean if a contact is in a list
    /// </summary>
    public class ContactCheckConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="values">[0] is Contact, [1] is Filters object</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>True if values missing, True if contact in list, False otherwise</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var contact = values[0] as Contact;
            var filters = values[1] as StreamModel;

            if (filters == null || contact == null)
            {
                return true;
            }

            Filter found = filters.GetFiltersFor(contact).FirstOrDefault();

            //Indetermine state of checkboxes == null
            if (found != null && found.IsIncluded == FilterBehaviour.Exclude)
                return null;

            return (found != null);
        }

        /// <summary>
        /// Convert Back (not implemented)
        /// </summary>
        /// <param name="value">Not used</param>
        /// <param name="targetTypes">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>Throws exception</returns>
        /// <exception cref="System.NotImplementedException">Not implemented</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class ContactColorConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="values">[0] is Contact, [1] is Filters object</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>True if values missing, True if contact in list, False otherwise</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var contact = values[0] as Contact;
            var filters = values[1] as StreamModel;

            if (filters == null || contact == null)
            {
                return null;
            }

            Filter found = filters.GetFiltersFor(contact).FirstOrDefault();

            //Indetermine state of checkboxes == null
            if (found != null)
            {
                return found.Color;
            }
            return false;
        }

        /// <summary>
        /// Convert Back (not implemented)
        /// </summary>
        /// <param name="value">Not used</param>
        /// <param name="targetTypes">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>Throws exception</returns>
        /// <exception cref="System.NotImplementedException">Not implemented</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}