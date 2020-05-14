using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchChatBot.Shared.Models
{
    public class TwitchWebhookFollowersResponse
    {
        [JsonProperty(PropertyName = "followed_at")]
        public DateTime FollowedAt { get; set; }
        [JsonProperty(PropertyName = "from_id")]
        public string FromId { get; set; }
        [JsonProperty(PropertyName = "from_name")]
        public string FromName { get; set; }
        [JsonProperty(PropertyName = "to_id")]
        public string ToId { get; set; }
        [JsonProperty(PropertyName = "to_name")]
        public string ToName { get; set; }
    }
}
