using System;
using MahApps.Twitter.Extensions;
using Newtonsoft.Json;

namespace MahApps.Twitter.Models
{
    public class DirectMessageContainer
    {
        [JsonProperty(PropertyName = "direct_message")]
        public DirectMessage DirectMessage { get; set; }
    }

    public class DirectMessage : ITwitterResponse
    {
        [JsonProperty(PropertyName = "sender_id")]
        public long? SenderId { get; set; }

        [JsonProperty(PropertyName = "recipient_id")]
        public long? RecipientId { get; set; }

        public long? Id { get; set; }

        public String Text { get; set; }

        [JsonProperty(PropertyName = "sender_screen_name")]
        public String SenderScreenName { get; set; }

        [JsonProperty(PropertyName = "sender")]
        public User Sender { get; set; }

        [JsonProperty(PropertyName = "recipient_screen_name")]
        public String RecipientScreenName { get; set; }

        [JsonProperty(PropertyName = "recipient")]
        public User Recipient { get; set; }

        [JsonProperty("created_at")]
        public object CreatedDate { get; set; }

        public DateTime Created
        {
            get { return CreatedDate.ToString().ParseDateTime(); }
        }
    }
}