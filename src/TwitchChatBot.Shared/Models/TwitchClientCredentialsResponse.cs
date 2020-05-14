using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TwitchChatBot.Shared.Models
{
    public class TwitchClientCredentialsResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token"), JsonIgnore]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("scope"), JsonIgnore]
        public string[] Scope { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}
