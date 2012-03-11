using System;

namespace MahTweets.Core.Media
{
    /// <summary>
    /// Media object to be loaded from plugin
    /// </summary>
    public class MediaResult
    {
        /// <summary>
        /// Type of media
        /// </summary>
        public MediaType MediaType { get; set; }

        /// <summary>
        /// Provider for media
        /// </summary>
        public IMediaProvider Provider { get; set; }

        /// <summary>
        /// Url of media
        /// </summary>
        public String Url { get; set; }

        /// <summary>
        /// Content
        /// </summary>
        public String Data { get; set; }
    }
}