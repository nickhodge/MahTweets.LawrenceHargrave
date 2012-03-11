namespace MahTweets.Core.Media
{
    /// <summary>
    /// Media types supported by plugins
    /// </summary>
    public enum MediaType
    {
        /// <summary>
        /// Image formats
        /// </summary>
        Image = 0x1,

        /// <summary>
        /// Audio formats
        /// </summary>
        Audio = 0x2,

        /// <summary>
        /// Video formats
        /// </summary>
        Video = 0x4,

        /// <summary>
        /// Flash format
        /// </summary>
        Flash = 0x8
    }
}