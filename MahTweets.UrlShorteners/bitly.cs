using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Utilities;

namespace MahTweets.UrlShorteners
{
    public class Bitly : IUrlShortener
    {
        public string Description
        {
            get { return ""; }
        }

        #region IUrlShortener Members

        public string Name
        {
            get { return "Bit.ly"; }
        }

        public async Task<string> Shorten(String Url)
        {
            if (Url == null) return null;

            var urlExpanders = CompositionManager.Get<IUrlExpandService>();

            if (urlExpanders.IsShortUrl(Url) || Url.Length < 20)
            {
                return Url;
            }

            if (Credentials == null)
            {
                var plugins = CompositionManager.Get<IPluginRepository>();
                IUrlShortener bitlyPlugin = plugins.UrlShorteners.FirstOrDefault(t => t.Protocol == Protocol);
                if (bitlyPlugin == null)
                {
                    ShowSettings(Url);
                    return null;
                }
                Credentials = bitlyPlugin.Credentials;
            }

            string url = string.Format("http://api.bit.ly/v3/shorten?format=xml&longUrl={0}&login={1}&apiKey={2}",
                                       Uri.EscapeDataString(Url), Credentials.Username, Credentials.Password);

            var _fetcher = new AsyncWebFetcher();
            string resultText = await _fetcher.FetchAsync(url);

            XDocument resultXml = XDocument.Parse(resultText);
            XElement status = resultXml.Descendants("status_code").First();
            if (status != null)
            {
                if (status.Value == "500")
                {
                    string errorMessage = "Unknown error";
                    try
                    {
                        errorMessage = resultXml.Descendants("status_txt").First().Value;
                    }
                    catch (Exception ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }


                    var _eventAggregator = CompositionManager.Get<IEventAggregator>();
                    _eventAggregator.GetEvent<ShowNotification>().Publish(
                        new ShowNotificationPayload("Error with bit.ly: " + errorMessage, TimeSpan.Zero,
                                                    NotificactionLevel.Error));

                    if (errorMessage == "INVALID_APIKEY")
                    {
                        Credentials = null;
                        var plugins = CompositionManager.Get<IPluginRepository>();
                        IUrlShortener bitlyPlugin = plugins.UrlShorteners.FirstOrDefault(t => t.Protocol == Protocol);
                        if (bitlyPlugin != null)
                            plugins.UrlShorteners.Remove(bitlyPlugin);
                    }
                }
                else
                {
                    IEnumerable<XElement> data = resultXml.Descendants("data");
                    if (data != null)
                    {
                        string x = (from result in data
                                    let xElement = result.Element("url")
                                    where xElement != null
                                    select xElement.Value).FirstOrDefault();

                        if (!String.IsNullOrEmpty(x))
                        {
                            return x;
                        }
                    }
                }
            }
            return Url;
        }

        public Boolean HasSettings
        {
            get { return true; }
        }

        public string Protocol
        {
            get { return "bitly"; }
        }

        public BitmapImage Icon
        {
            get
            {
                return
                    ImageUtility.ConvertResourceToBitmap(
                        "pack://application:,,,/MahTweets.UrlShorteners;component/bitly.png");
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
            IUrlShortener bitlyPlugin = plugins.UrlShorteners.FirstOrDefault(t => t.Protocol == Protocol);

            if (Credentials == null && bitlyPlugin != null)
                Credentials = bitlyPlugin.Credentials;

            var setupView = new BitlyLoginView(Credentials);
            setupView.Closed += (s, e) => setupView_Closed(setupView, Url);
            var _eventAggregator = CompositionManager.Get<IEventAggregator>();
            _eventAggregator.Show(setupView);
        }

        private Task<string> setupView_Closed(BitlyLoginView view, String Url)
        {
            if (view.ModalResult != true) return null;
            Credentials = view.Credentials;
            var _eventAggregator = CompositionManager.Get<IEventAggregator>();
            _eventAggregator.GetEvent<UrlShortenerAdded>().Publish(this);

            return Shorten(Url);
        }
    }
}