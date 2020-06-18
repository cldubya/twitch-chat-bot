using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Web.v2.Hubs
{
    public class TwitchHub : Hub
    {
        public async Task StreamUpdate(ChannelActivityEntity entity)
        {
            await Clients.All.SendAsync("StreamUpdate", entity);
        }
    }
}
