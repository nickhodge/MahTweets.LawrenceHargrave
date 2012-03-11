using System;
using Newtonsoft.Json;

namespace MahApps.Twitter.Models
{
    public class RelationShipEntity
    {
        [JsonProperty(PropertyName = "want_retweets", NullValueHandling = NullValueHandling.Ignore)]
        public bool WantRetweets { get; set; }

        [JsonProperty(PropertyName = "marked_spam", NullValueHandling = NullValueHandling.Ignore)]
        public bool MarkedSpam { get; set; }

        [JsonProperty(PropertyName = "blocking", NullValueHandling = NullValueHandling.Ignore)]
        public bool Blocking { get; set; }

        [JsonProperty(PropertyName = "all_replies", NullValueHandling = NullValueHandling.Ignore)]
        public bool AllReplies { get; set; }

        [JsonProperty(PropertyName = "notifications_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool NotificationsEnabled { get; set; }

        [JsonProperty(PropertyName = "followed_by", NullValueHandling = NullValueHandling.Ignore)]
        public bool FollowedBy { get; set; }

        [JsonProperty(PropertyName = "following", NullValueHandling = NullValueHandling.Ignore)]
        public bool Following { get; set; }

        [JsonProperty(PropertyName = "can_dm", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanDm { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty(PropertyName = "screen_name")]
        public String ScreenName { get; set; }
    }
}