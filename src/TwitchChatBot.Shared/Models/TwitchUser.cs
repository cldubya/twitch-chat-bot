using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TwitchChatBot.Shared.Models
{
    public class TwitchUser
    {
        public string Id { get; set; }
        [JsonProperty("login")]
        public string LoginName { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Type { get; set; }
        [JsonProperty("broadcaster_type")]
        public string BroadcasterType { get; set; }
        public string Description { get; set; }
        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }
        [JsonProperty("offline_image_url")]
        public string OfflineImageUrl { get; set; }
        [JsonProperty("view_count")]
        public int ViewCount { get; set; }
    }
}
