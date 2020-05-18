using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Fx
{
    public class Functions
    {
        [FunctionName("negotiate")]
        public SignalRConnectionInfo GetSignalRInfo(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
           [SignalRConnectionInfo(HubName = Constants.FX_SIGNALR_HUB_NAME)] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("messages")]
        public Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,
            [SignalR(HubName = Constants.FX_SIGNALR_HUB_NAME)] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "StreamUpdate",
                    Arguments = new[] { message }
                });
        }
    }
}
