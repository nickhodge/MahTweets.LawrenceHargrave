using System;
using System.Threading.Tasks;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.MediaProviders
{
    public class ImageProvider : IMediaProvider
    {
        #region IMediaProvider Members

        public String Name
        {
            get { return "Image"; }
        }

        public bool Match(string url)
        {
            return (url.ToLower().EndsWith(".jpg") || url.ToLower().EndsWith(".png"));
        }

        public Task<string> Transform(string url)
        {
            return Task.Run(() => url);
        }

        #endregion
    }
}