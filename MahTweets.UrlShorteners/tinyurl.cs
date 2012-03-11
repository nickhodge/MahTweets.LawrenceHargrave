using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Utilities;

namespace MahTweets.UrlShorteners
{
    public class TinyUrl : IUrlShortener
    {
        #region IUrlShortener Members

        public bool HasSettings
        {
            get { return false; }
        }

        public string Name
        {
            get { return "TinyURL"; }
        }

        public string Protocol
        {
            get { return "tinyurl"; }
        }

        public BitmapImage Icon
        {
            get
            {
                return
                    ImageUtility.ConvertResourceToBitmap(
                        "pack://application:,,,/MahTweets.UrlShorteners;component/tinyurl.jpg");
            }
        }

        public Credential Credentials { get; set; }

        public string Id
        {
            get { return Name; }
        }

        public void Setup()
        {
        }

        public async Task<string> Shorten(String Url)
        {
            try
            {
                if (Url.Length <= 30)
                {
                    return Url;
                }

                var _fetcher = new AsyncWebFetcher();
                string text =
                    await _fetcher.FetchAsync("http://tinyurl.com/api-create.php?url=" + Uri.EscapeDataString(Url));

                return text;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                return Url;
            }
        }


        public void ShowSettings()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}