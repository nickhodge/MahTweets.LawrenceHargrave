using System;
using MahApps.Twitter.Extensions;
using Newtonsoft.Json;

namespace MahApps.Twitter.Models
{
    public class User : ITwitterResponse
    {
        public static Action<User> DoNothing = user => { };

        [JsonProperty(PropertyName = "screen_name")]
        public String ScreenName { get; set; }

        [JsonProperty(PropertyName = "show_all_inline_media")]
        public bool ShowAllInlineMedia { get; set; }

        [JsonProperty(PropertyName = "contributors_enabled")]
        public bool ContributorsEnabled { get; set; }

        [JsonProperty(PropertyName = "profile_background_tile")]
        public bool ProfileBackgroundTile { get; set; }

        [JsonProperty(PropertyName = "profile_sidebar_border_color")]
        public String ProfileSidebarBorderColour { get; set; }

        [JsonProperty(PropertyName = "profile_use_background_image")]
        public bool ProfileUseBackgroundimage { get; set; }

        [JsonProperty(PropertyName = "profile_background_color")]
        public String ProfileBackgroundColour { get; set; }

        [JsonProperty(PropertyName = "profile_sidebar_fill_color")]
        public String ProfileSidebarFillColour { get; set; }

        [JsonProperty(PropertyName = "profile_link_color")]
        public String ProfileLinkColour { get; set; }

        [JsonProperty(PropertyName = "profile_image_url")]
        public String ProfileImageUrl { get; set; }

        [JsonProperty(PropertyName = "profile_background_image_url")]
        public String ProfileBackgroundImageUrl { get; set; }

        [JsonProperty(PropertyName = "profile_text_color")]
        public String ProfileTextColour { get; set; }

        public string Lang { get; set; }

        [JsonProperty(PropertyName = "following", NullValueHandling = NullValueHandling.Ignore)]
        public bool Following { get; set; }

        [JsonProperty(PropertyName = "followers_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? FollowersCount { get; set; }

        public String Description { get; set; }

        public String Location { get; set; }

        public bool Verified { get; set; }

        [JsonProperty(PropertyName = "friends_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? FriendsCount { get; set; }

        [JsonProperty(PropertyName = "geo_enabled")]
        public bool GeoEnabled { get; set; }

        [JsonProperty(PropertyName = "favourites_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? FavouritesCount { get; set; }

        public bool Protected { get; set; }

        public String Url { get; set; }

        public String Name { get; set; }

        [JsonProperty(PropertyName = "listed_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? ListedCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty(PropertyName = "statuses_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? StatusCount { get; set; }

        [JsonProperty(PropertyName = "utc_offset", NullValueHandling = NullValueHandling.Ignore)]
        public long? UTCOffset { get; set; }

        [JsonProperty("created_at")]
        public object CreatedDate { get; set; }

        public DateTime Created
        {
            get { return CreatedDate.ToString().ParseDateTime(); }
        }
    }
}