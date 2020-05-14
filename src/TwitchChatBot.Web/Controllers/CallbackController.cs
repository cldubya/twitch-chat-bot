using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TwitchChatBot.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class CallbackController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        public CallbackController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery]string code)
        {
            await Task.CompletedTask;

            var data = new Dictionary<string, string>()
            {
                {"client_id","" },
                {"client_secret",""},
                {"code","" },
                {"grant_type","authorization_code" },
                {"redirect_uri","" }
            };

            return NoContent();
        }
    }
}
