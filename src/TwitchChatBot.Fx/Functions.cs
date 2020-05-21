using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Fx
{
    public class Functions
    {
        [FunctionName("negotiate")]
        public SignalRConnectionInfo GetSignalRInfo(
           [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
           [SignalRConnectionInfo(HubName = Constants.FX_CONFIG_SIGNALR_HUB_VALUE)] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("stream-bot-message")]
        public Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,
            [SignalR(HubName = Constants.FX_CONFIG_SIGNALR_HUB_VALUE)] IAsyncCollector<SignalRMessage> signalRMessages)
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
