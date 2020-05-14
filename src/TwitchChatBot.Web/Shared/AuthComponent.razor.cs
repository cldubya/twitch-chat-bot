using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TwitchChatBot.Web.Shared
{
    public partial class AuthComponent
    {
        [Inject]
        protected IHttpContextAccessor HttpContextAccessor { get; set; }

        public async Task Login()
        {
            var props = new AuthenticationProperties();
            props.RedirectUri = "/oldcallback";

            await HttpContextAccessor.HttpContext.ChallengeAsync("twitch", props);
        }

        public async Task Logout()
        {

        }
    }
}
