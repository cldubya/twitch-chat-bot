using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchChatBot.Shared.Models
{
    public class TwitchWebhookRequest
    {
        [JsonProperty("hub.callback")]
        public string Callback { get; set; }
        [JsonProperty("hub.mode")]
        public string Mode { get; set; }
        [JsonProperty("hub.topic")]
        public string Topic { get; set; }
        [JsonProperty("hub.lease_seconds")]
        public int Lease { get; set; }
        //[JsonProperty("hub.secret", Required = Required.DisallowNull)]
        //public string Secret { get; set; }
    }
}
