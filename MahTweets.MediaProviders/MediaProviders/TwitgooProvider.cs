using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class TwitgooProvide : IMediaProvider
    {
        #region IMediaProvider Members

        public string Name
        {
            get { return "twitgoo"; }
        }

        public bool Match(string url)
        {
            return url.Contains("twitgoo.com/");
        }

        public async Task<string> Transform(string url)
        {
            var match = Regex.Match(url, "twitgoo.com/([A-Za-z0-9]*)");

            if (!match.Success)
                return null;
            var twitgooID = match.Groups[1].Value;

            var fetcher = new AsyncWebFetcher();
            var text = await fetcher.FetchAsync("http://twitgoo.com/api/message/info/" + twitgooID);
            var xdoc = new XmlDocument();
            xdoc.LoadXml(text);

            try
            {
                var selectSingleNode = xdoc.SelectSingleNode("/rsp/imageurl");
                if (selectSingleNode != null)
                {
                    var r = selectSingleNode.InnerText;
                    return r;
                }
            } catch (Exception ex)
            {
                
            }
            return null;
        }

        #endregion
    }
}