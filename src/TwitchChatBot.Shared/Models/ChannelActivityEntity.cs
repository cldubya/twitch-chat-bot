using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchChatBot.Shared.Enums;
using TwitchLib.Api.V5.Models.Channels;

namespace TwitchChatBot.Shared.Models
{
    public class ChannelActivityEntity : TableEntity
    {
        public string Viewer { get; set; }
        public string Activity { get; set; }

        public ChannelActivityEntity()
        {

        }
    }
}
