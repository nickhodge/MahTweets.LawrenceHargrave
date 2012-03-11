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
    public class IsGd : IUrlShortener
    {
        private const string APIUrl = "http://is.gd/api.php?longurl=";
        private readonly AsyncWebFetcher _fetcher;

        public IsGd()
        {
            _fetcher = new AsyncWebFetcher();
        }

        #region IUrlShortener Members

        public bool HasSettings
        {
            get { return false; }
        }

        public string Name
        {
            get { return "is.gd"; }
        }

        public string Protocol
        {
            get { return "is.gd"; }
        }

        public BitmapImage Icon
        {
            get
            {
                return
                    ImageUtility.ConvertResourceToBitmap(
                        "pack://application:,,,/MahTweets.UrlShorteners;component/isgd.jpg");
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
                string text = await _fetcher.FetchAsync(APIUrl + Uri.EscapeDataString(Url));
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
        }

        #endregion
    }
}