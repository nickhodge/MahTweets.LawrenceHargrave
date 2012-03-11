using System;
using Newtonsoft.Json;

namespace MahApps.Twitter.Models
{
    public class Delete : ITwitterResponse
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
    }

    public class StreamEvent : ITwitterResponse
    {
        [JsonProperty(PropertyName = "target")]
        public User TargetUser { get; set; }

        [JsonProperty(PropertyName = "source")]
        public User SourceUser { get; set; }

        [JsonProperty(PropertyName = "target_object")]
        public Tweet Target { get; set; }

        public String Event { get; set; }
    }
}