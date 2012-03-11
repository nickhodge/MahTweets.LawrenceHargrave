using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MahTweets.Core.Composition;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.UI.Converters
{
    public class StreamCheckConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var filters = values[0] as StreamModel;
            var microblog = values[1] as IMicroblog;
            var updateType = values[2] as UpdateType;

            if (filters == null)
            {
                //Logging.Important("StreamCheckConverter: Filters not found");
                return null;
            }

            if (microblog == null)
            {
                //Logging.Important("StreamCheckConverter: Microblog not found");
                return null;
            }

            if (updateType == null)
            {
                //Logging.Important("StreamCheckConverter: UpdateType not found");
                return null;
            }

            try
            {
                Filter found = filters.GetFiltersFor(microblog, updateType).FirstOrDefault();

                if (found == null) return false;
                if (found.IsIncluded == FilterBehaviour.Exclude) return null;
                if (found.IsIncluded == FilterBehaviour.Include) return true;
                if (found.IsIncluded == FilterBehaviour.NoBehaviour) return false;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class StreamColourConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var filters = values[0] as StreamModel;
            var microblog = values[1] as IMicroblog;
            var updateType = values[2] as UpdateType;

            if (filters == null)
            {
                //Logging.Important("StreamCheckConverter: Filters not found");
                return null;
            }

            if (microblog == null)
            {
                //Logging.Important("StreamCheckConverter: Microblog not found");
                return null;
            }

            if (updateType == null)
            {
                //Logging.Important("StreamCheckConverter: UpdateType not found");
                return null;
            }

            try
            {
                Filter found = filters.GetFiltersFor(microblog, updateType).FirstOrDefault();

                if (found == null) return false;
                return found.Color;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}