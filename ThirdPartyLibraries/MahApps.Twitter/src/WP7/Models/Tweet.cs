using System;
using System.Collections.Generic;
using System.ComponentModel;
using MahApps.Twitter.Extensions;
using Newtonsoft.Json;

namespace MahApps.Twitter.Models
{
    public class SiteStreamsWrapper : ITwitterResponse
    {
        [JsonProperty(PropertyName = "for_user")]
        public string ForUser { get; set; }

        [JsonProperty(PropertyName = "message")]
        public SiteStreamsMessage Message { get; set; }
    }

    public class SiteStreamsMessage : Tweet
    {
        [JsonProperty(PropertyName = "direct_message")]
        public DirectMessage DirectMessage { get; set; }
    }

    public class Tweet : ITwitterResponse
    {
        [JsonProperty(PropertyName = "retweeted_status")]
        public Tweet RetweetedStatus { get; set; }

        public String Text { get; set; }

        [JsonProperty(PropertyName = "in_reply_to_screen_name", NullValueHandling = NullValueHandling.Ignore)]
        public String InReplyToScreenName { get; set; }

        [JsonProperty(PropertyName = "favorited")]
        public bool Favourited { get; set; }

        public String Source { get; set; }

        public User User { get; set; }

        public Geo Geo { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "in_reply_to_status_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? InReplyToStatusId { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "in_reply_to_user_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? InReplyToUserId { get; set; }

        public bool Truncated { get; set; }

        public Geo Coordinates { get; set; }

        public long? Id { get; set; }

        [JsonProperty("created_at")]
        public object CreatedDate { get; set; }

        public DateTime Created
        {
            get { return CreatedDate.ToString().ParseDateTime(); }
        }

        [JsonProperty("entities")]
        public Entity Entities { get; set; }
    }

    public class Entity
    {
        [JsonProperty("user_mentions")]
        public List<UserMention> UserMentions { get; set; }

        [JsonProperty("urls")]
        public List<Url> Urls { get; set; }

        [JsonProperty("hashtags")]
        public List<Hashtag> Hashtags { get; set; }

        [JsonProperty("media")]
        public List<Media> Media { get; set; }
    }

    public class Media
    {
        [JsonProperty("media_url")]
        public String MediaURL { get; set; }

        [JsonProperty("expanded_url")]
        public String ExpandedURL { get; set; }

        [JsonProperty("type")]
        public String Type { get; set; }
    }

    public class UserMention
    {
        public String ID { get; set; }

        [JsonProperty("screen_name")]
        public String ScreenName { get; set; }

        public String Name { get; set; }

        [JsonProperty("indices")]
        public Int32[] Indices { get; set; }
    }

    public class Url
    {
        [JsonProperty("indices")]
        public Int32[] Indices { get; set; }

        [JsonProperty("url")]
        public String Link { get; set; }

        [JsonProperty("display_url")]
        public String DisplayUrl { get; set; }

        [JsonProperty("expanded_url")]
        public String ExpandedUrl { get; set; }
    }

    public class Hashtag
    {
        public String Text { get; set; }

        [JsonProperty("indices")]
        public Int32[] Indices { get; set; }
    }
}