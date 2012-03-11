using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.UI.Converters
{
    public class BitmapIgnoreMetadataConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return GetBitMapAsync(value as Uri, false).Result;
            }
            catch (IOException ioex)
            {
                return GetBitMapAsync(value as Uri, true).Result;
                    // sometimes, another thread is in the middle of getting an image. WPF barfs, and throws an exception. here, we use the refactored code below to wait a second, then retry. Using the Asyc Library to do this without blocking the UI thread. Oh thankyou Asych gods!
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().
                    ReportHandledException(ex);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion

        private static async Task<object> GetBitMapAsync(Uri uri, bool waiting)
        {
            if (waiting) await Task.Delay(1000); // wait a sec!
            var urithingy = uri;
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnDemand;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.UriSource = urithingy;
            image.EndInit();
            return image;
        }
    }
}