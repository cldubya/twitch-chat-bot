using Microsoft.AspNetCore.Mvc;

namespace TwitchChatBot.Shared.Models
{
    public class TwitchWebhookResponse
    {
        [BindProperty(Name ="hub.challenge")]
        public string Challenge { get; set; }
        [BindProperty(Name = "hub.lease_seconds")]
        public int Lease { get; set; }
        [BindProperty(Name = "hub.mode")]
        public string Mode { get; set; }
        [BindProperty(Name = "hub.topic")]
        public string Topic { get; set; }
    }
}
