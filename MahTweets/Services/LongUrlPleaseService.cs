using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using Newtonsoft.Json;

namespace MahTweets.Services
{
    public class LongUrlPleaseService : IUrlExpandService
    {
        private const string LongUrlPleaseUri = "http://www.longurlplease.com/api/v1.1?{0}";
        private const string UrlCacheFilename = "Resolved LongURLs.cache";

        private static Dictionary<string, LongUrlCacheEntry> _urlCache;
        private static bool _urlCacheInitialized;
        private static DateTime _urlCacheLastFlush = new DateTime(1900, 1, 1);
        private static bool _urlCacheDirty;
        private static IStorage _storage;

        #region ALL THE SHORT URLS

        private static readonly string[] ShortUrlSources =
            new[]
                {
                    "http://bit.ly/",
                    "http://cli.gs/",
                    "http://is.gd/",
                    "http://j.mp/",
                    "http://kl.am/",
                    "http://su.pr/",
                    "http://tinyurl.com/",
                    "http://307.to/",
                    "http://adjix.com/",
                    "http://b23.ru/",
                    "http://bacn.me/",
                    "http://bloat.me/",
                    "http://budurl.com/",
                    "http://clipurl.us/",
                    "http://cort.as/",
                    "http://dwarfurl.com/",
                    "http://ff.im/",
                    "http://fff.to/",
                    "http://href.in/",
                    "http://idek.net/",
                    "http://korta.nu/",
                    "http://lin.cr/",
                    "http://ln-s.net/",
                    "http://loopt.us/",
                    "http://lost.in/",
                    "http://memurl.com/",
                    "http://merky.de/",
                    "http://migre.me/",
                    "http://moourl.com/",
                    "http://nanourl.se/",
                    "http://ow.ly/",
                    "http://peaurl.com/",
                    "http://ping.fm/",
                    "http://piurl.com/",
                    "http://plurl.me/",
                    "http://pnt.me/",
                    "http://poprl.com/",
                    "http://post.ly/",
                    "http://rde.me/",
                    "http://reallytinyurl.com/",
                    "http://redir.ec/",
                    "http://retwt.me/",
                    "http://rubyurl.com/",
                    "http://short.ie/",
                    "http://short.to/",
                    "http://smallr.com/",
                    "http://sn.im/",
                    "http://sn.vc/",
                    "http://snipr.com/",
                    "http://snipurl.com/",
                    "http://snurl.com/",
                    "http://tiny.cc/",
                    "http://tinysong.com/",
                    "http://togoto.us/",
                    "http://tr.im/",
                    "http://tra.kz/",
                    "http://trg.li/",
                    "http://twurl.cc/",
                    "http://twurl.nl/",
                    "http://u.mavrev.com/",
                    "http://u.nu/",
                    "http://ur1.ca/",
                    "http://url.az/",
                    "http://url.ie/",
                    "http://urlx.ie/",
                    "http://w34.us/",
                    "http://xrl.us/",
                    "http://yep.it/",
                    "http://zi.ma/",
                    "http://zurl.ws/",
                    "http://chilp.it/",
                    "http://notlong.com/",
                    "http://qlnk.net/",
                    "http://trim.li/",
                    "http://hnsl.mn/",
                    "http://tek.ms/",
                    "http://t.co/",
                    "http://digg.com/",
                    "http://fb.me/",
                    "http://goo.gl/",
                    "http://dFL8.me/",
                    "http://livesi.de/",
                    "http://om.ly/"
                };

        #endregion

        public LongUrlPleaseService()
        {
            _storage = CompositionManager.Get<IStorage>();
        }

        #region IUrlExpandService Members

        public async Task<string> ExpandUrl(string shortUrl)
        {
            string longUrl = shortUrl; // just in case things go pear shaped, ready with the same return

            if (!_urlCacheInitialized)
            {
                if (_storage.Exists(UrlCacheFilename))
                {
                    _urlCache = await _storage.Load<Dictionary<string, LongUrlCacheEntry>>(UrlCacheFilename);
                }

                if (_urlCache == null)
                {
                    _urlCache = new Dictionary<string, LongUrlCacheEntry>();
                    _storage.Store(_urlCache, UrlCacheFilename, null);
                }

                _urlCacheInitialized = true;
            }

            if (_urlCache.ContainsKey(shortUrl))
            {
                LongUrlCacheEntry entry = _urlCache[shortUrl];
                entry.SetUsed();
                longUrl = entry.LongUrl;
                return longUrl;
            }

            var fetch = new List<string> {shortUrl};

            Dictionary<string, string> fetchResults = await GetLongUrlPleaseRequest(fetch);

            if (fetchResults != null)
            {
                foreach (var url in fetchResults)
                {
                    if (!_urlCache.ContainsKey(url.Key))
                    {
                        _urlCache.Add(url.Key, new LongUrlCacheEntry(url.Value));
                        _urlCacheDirty = true;
                    }
                    longUrl = url.Value;
                }

                if (_urlCacheDirty && DateTime.UtcNow.Subtract(_urlCacheLastFlush).TotalSeconds > 30)
                {
                    foreach (
                        string url in
                            from url in _urlCache.Keys.ToList()
                            let entry = _urlCache[url]
                            where entry.Expired
                            select url)
                    {
                        _urlCache.Remove(url);
                    }

                    _storage.Store(_urlCache, UrlCacheFilename, null);
                    _urlCacheDirty = false;
                    _urlCacheLastFlush = DateTime.UtcNow;
                }
            }
            return longUrl;
        }

        public bool IsShortUrl(string shortUrl)
        {
            if (shortUrl == null) return false;
            bool result = ShortUrlSources.AsParallel().Any(shortUrl.Contains);
            return result;
        }

        #endregion

        private async Task<Dictionary<string, string>> GetLongUrlPleaseRequest(IEnumerable<string> shortUrls)
        {
            Uri longUrlPleaseUri = GetLongUrlPleaseUri(shortUrls);
            var _fetcher = new AsyncWebFetcher();
            try
            {
                string responseText = await _fetcher.FetchAsync(longUrlPleaseUri);
                var responseObject = JsonConvert.DeserializeObject<Dictionary<String, String>>(responseText);
                return responseObject;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            return null;
        }

        private static Uri GetLongUrlPleaseUri(IEnumerable<String> shortUrls)
        {
            // Single or multiple short URLs may be requested.             
            // http://www.longurlplease.com/api/v1.1?q=http://bit.ly/enAo&q=http://short.ie/bestxkcd

            var parameters = new StringBuilder();

            string delimiter = string.Empty;
            foreach (string url in shortUrls)
            {
                parameters.Append(delimiter).AppendFormat("q={0}", url);
                delimiter = "&";
            }

            return new Uri(string.Format(LongUrlPleaseUri, parameters), UriKind.Absolute);
        }
    }

    [DataContract]
    public class LongUrlCacheEntry
    {
        public LongUrlCacheEntry(string longUrl)
        {
            Created = DateTime.UtcNow;
            LastUsed = DateTime.UtcNow;
            LongUrl = longUrl;
        }

        [DataMember]
        public DateTime Created { get; private set; }

        [DataMember]
        public DateTime LastUsed { get; private set; }

        [DataMember]
        public string LongUrl { get; private set; }

        public bool Expired
        {
            get { return DateTime.UtcNow.Subtract(LastUsed).TotalDays > 14; }
        }

        public void SetUsed()
        {
            LastUsed = DateTime.UtcNow;
        }
    }
}