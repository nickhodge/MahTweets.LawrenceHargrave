using System.Net;

namespace MahTweets.Core.Extensions
{
    public static class WebClientExtensions
    {
        public static void UseDefaultProxy(this WebClient wc)
        {
            wc.Proxy = WebRequest.DefaultWebProxy;
            wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
    }
}