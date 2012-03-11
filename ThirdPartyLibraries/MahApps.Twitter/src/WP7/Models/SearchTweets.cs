using System.ComponentModel;
using Newtonsoft.Json;

namespace MahApps.Twitter.Models
{
    public class SearchTweet : Tweet
    {
        [JsonProperty(PropertyName = "to_user", NullValueHandling = NullValueHandling.Ignore)]
        public new string InReplyToScreenName { get; set; }

        [JsonProperty(PropertyName = "from_user")]
        public string ContactName { get; set; }

        [JsonProperty(PropertyName = "profile_image_url")]
        public string ContactImage { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "to_user_id", NullValueHandling = NullValueHandling.Ignore)]
        public new long? InReplyToUserId { get; set; }
    }
}