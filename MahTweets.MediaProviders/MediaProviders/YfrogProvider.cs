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
            Match match = Regex.Match(url, "yfrog.com/([A-Za-z0-9]*)");
            if (match.Success)
            {
                string yfrogId = match.Groups[1].Value;

                var _fetcher = new AsyncWebFetcher();
                string text = await _fetcher.FetchAsync("http://yfrog.com/api/xmlInfo?path=" + yfrogId);
                var xdoc = new XmlDocument();
                xdoc.LoadXml(text);

                var xnm = new XmlNamespaceManager(xdoc.NameTable);
                xnm.AddNamespace("is", "http://ns.imageshack.us/imginfo/7/");

                XmlNode selectSingleNode = xdoc.SelectSingleNode("/is:imginfo/is:links/is:image_link", xnm);
                if (selectSingleNode != null)
                {
                    string r = selectSingleNode.InnerText;
                    return r;
                }
                return null;
            }
            return null;
        }

        #endregion
    }
}