using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Utilities;
using Newtonsoft.Json.Linq;

namespace MahTweets.UrlShorteners
{
    public class Yourls : IUrlShortener
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPluginRepository _pluginRepository;

        [ImportingConstructor]
        public Yourls(IEventAggregator eventAggregator, IPluginRepository pluginRepository)
        {
            _eventAggregator = eventAggregator;
            _pluginRepository = pluginRepository;
        }

        public string Description
        {
            get { return ""; }
        }

        #region IUrlShortener Members

        public string Name
        {
            get { return "your.ls"; }
        }

        public async Task<string> Shorten(String Url)
        {
            var urlExpanders = CompositionManager.Get<IUrlExpandService>();

            if (urlExpanders.IsShortUrl(Url) || Url.Length < 20)
            {
                return Url;
            }

            if (Credentials == null)
            {
                IUrlShortener yoursPlugin = _pluginRepository.UrlShorteners.FirstOrDefault(t => t.Protocol == Protocol);
                if (yoursPlugin == null)
                {
                    ShowSettings(Url);
                }
                Credentials = yoursPlugin.Credentials;
            }


            try
            {
                string url =
                    string.Format("{0}/yourls-api.php?action=shorturl&format=json&url={1}&username={2}&password={3}",
                                  Credentials.AccountName, Uri.EscapeDataString(Url), Credentials.Username,
                                  Credentials.Password);
                var _fetcher = new AsyncWebFetcher();
                string result = await _fetcher.FetchAsync(url);

                JObject o = JObject.Parse(result);
                var x = (string) o.SelectToken("shorturl");

                if (String.IsNullOrEmpty(x)) return null;
                return x;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ShowNotification>().Publish(
                    new ShowNotificationPayload("Error with your.ls: " + ex.Message, TimeSpan.Zero,
                                                NotificactionLevel.Error));
                return Url;
            }
        }

        public Boolean HasSettings
        {
            get { return true; }
        }

        public string Protocol
        {
            get { return "yourls"; }
        }

        public BitmapImage Icon
        {
            get
            {
                return
                    ImageUtility.ConvertResourceToBitmap(
                        "pack://application:,,,/MahTweets.UrlShorteners;component/yourls-logo.png");
            }
        }

        public Credential Credentials { get; set; }

        public string Id
        {
            get { return Name; }
        }

        public void ShowSettings()
        {
            ShowSettings(String.Empty);
        }

        public void Setup()
        {
        }

        #endregion

        private void ShowSettings(string Url)
        {
            var plugins = CompositionManager.Get<IPluginRepository>();
            IUrlShortener yourlsPlugin = plugins.UrlShorteners.FirstOrDefault(t => t.Protocol == Protocol);


            if (Credentials == null && yourlsPlugin != null)
                Credentials = yourlsPlugin.Credentials;

            var setupView = new YourlsLoginView(Credentials);
            setupView.Closed += (s, e) => SetupViewClosed(setupView, Url);

            _eventAggregator.Show(setupView);
        }

        private Task<string> SetupViewClosed(YourlsLoginView view, String Url)
        {
            if (view.ModalResult == true)
            {
                Credentials = view.Credentials;
                _eventAggregator.GetEvent<UrlShortenerAdded>().Publish(this);

                return Shorten(Url);
            }
            else
            {
                return null;
            }
        }
    }
}