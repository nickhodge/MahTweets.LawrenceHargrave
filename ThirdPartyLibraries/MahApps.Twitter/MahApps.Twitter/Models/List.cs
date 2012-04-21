using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MahApps.Twitter.Models
{
    public class ListResult : ITwitterResponse
    {
        public List<TwitterList> Lists { get; set; }
    }

    public class TwitterList : ITwitterResponse
    {
        public string Mode { get; set; }

        public string Description { get; set; }

        public string Uri { get; set; }

        [JsonProperty(PropertyName = "member_count")]
        public Int32 MemberCounter { get; set; }

        public String Slug { get; set; }

        public String Name { get; set; }

        [JsonProperty(PropertyName = "full_name")]
        public String FullName { get; set; }

        [JsonProperty(PropertyName = "user")]
        public User Owner { get; set; }

        [JsonProperty(PropertyName = "subscriber_count")]
        public long Subscribers { get; set; }

        public long Id { get; set; }

        public bool Following { get; set; }
    }
}