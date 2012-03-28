using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MahTweets.Core;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class YfrogProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public string Name
        {
            get { return "YFrog"; }
        }

        public bool Match(string url)
        {
            return url.Contains("http://yfrog.com/");
        }

        public async Task<string> Transform(string url)
        {
            var match = Regex.Match(url, "yfrog.com/([A-Za-z0-9]*)");
            if (match.Success)
            {
                var yfrogId = match.Groups[1].Value;

                var fetcher = new AsyncWebFetcher();
                var text = await fetcher.FetchAsync("http://yfrog.com/api/xmlInfo?path=" + yfrogId);
                var xdoc = new XmlDocument();
                try
                {
                    xdoc.LoadXml(text);

                    var xnm = new XmlNamespaceManager(xdoc.NameTable);
                    xnm.AddNamespace("is", "http://ns.imageshack.us/imginfo/7/");

                    var selectSingleNode = xdoc.SelectSingleNode("/is:imginfo/is:links/is:image_link", xnm);
                    if (selectSingleNode != null)
                    {
                        var r = selectSingleNode.InnerText;
                        return r;
                    }
                } catch(Exception ex)
                {
                    
                }
            }
            return null;
        }

        #endregion
    }
}