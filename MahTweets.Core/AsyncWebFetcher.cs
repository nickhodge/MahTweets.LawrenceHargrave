using System;
using System.Net;
using System.Threading.Tasks;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core
{
    public class AsyncWebFetcher
    {
        private readonly WebClient _wc;

        public AsyncWebFetcher()
        {
            _wc = new WebClient();
            _wc.UseDefaultProxy();
        }

        #region Async versions

        public async Task<string> FetchAsync(Uri uriToFetch)
        {
            return await FetchAsync(uriToFetch.ToString());
        }

        public async Task<string> FetchAsync(string urlToFetch)
        {
            string fetched = null;
            if (urlToFetch == null) return null;
            try
            {
                fetched = await _wc.DownloadStringTaskAsync(urlToFetch);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            return fetched;
        }

        public void FetchAndStoreAsync(Uri uriToFetch, string pathToStorageLocation)
        {
            if (uriToFetch == null) return;
            try
            {
                _wc.DownloadFileAsync(uriToFetch, pathToStorageLocation);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        public void FetchAndStoreAsync(string uriToFetch, string pathToStorageLocation)
        {
            var uri = new Uri(uriToFetch);
            FetchAndStoreAsync(uri, pathToStorageLocation);
        }

        public async Task<string> GetRedirectAsync(string url)
        {
            var wreq = (HttpWebRequest) WebRequest.Create(url);
            wreq.AllowAutoRedirect = false;
            var wresp = await wreq.GetResponseAsync();
            var x = wresp.Headers["Location"];
            wresp.Close();
            return x ?? url;
        }

        #endregion

    }
}