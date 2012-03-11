using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MahTweets.Core.Utilities
{
    public static class ImageUtility
    {
        public static bool IsValidImage(string fileName)
        {
            if (fileName.ToLower().EndsWith(".jpg") ||
                fileName.ToLower().EndsWith(".jpeg") ||
                fileName.ToLower().EndsWith(".gif") ||
                fileName.ToLower().EndsWith(".png"))
            {
                return true;
            }
#if DEBUG
            Console.WriteLine("{0} was encountered", fileName.ToLower());
#endif
            return false;
        }

        public static BitmapImage ConvertResourceToBitmap(string resource)
        {
            var u = new Uri(resource);
            return GetBitMapAsync(u, false).Result;
        }

        private static async Task<BitmapImage> GetBitMapAsync(Uri uri, bool waiting)
        {
            if (waiting) await Task.Delay(1000); // wait a sec!
            Uri urithingy = uri;
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