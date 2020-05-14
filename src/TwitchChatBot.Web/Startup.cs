using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Services;
using TwitchChatBot.Web.Data;
using TwitchLib.PubSub.Models.Responses;

namespace TwitchChatBot.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<IStorageService, AzureTableStorageService>();
            services.AddSingleton<ITwitchService, TwitchService>();
            services.AddSingleton<IBotService, BotService>();

            services.AddSingleton<NotifierService>();

            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddAuthentication(opts =>
            {
                opts.DefaultChallengeScheme = "twitch";
            })
            /*.AddTwitch("twitch",opts => 
            {
                opts.ClientId = Configuration["Values:Twitch:ClientId"];
                opts.ClientSecret = Configuration["Values:Twitch:ClientSecret"];
                opts.SignInScheme = IdentityConstants.ExternalScheme;

                opts.CallbackPath = "/oldcallback";

                opts.Scope.Clear();
                opts.Scope.Add("openid");
                opts.Scope.Add("user:read:email");
                opts.Scope.Add("analytics:read:games");
                opts.SaveTokens = true;               
            }*/
            .AddOpenIdConnect("twitch", opts =>
            {
                opts.Authority = "https://id.twitch.tv/oauth2";
                opts.ClientId = Configuration["Values:Twitch:ClientId"];
                opts.ClientSecret = Configuration["Values:Twitch:ClientSecret"];

                opts.CallbackPath = "/callback";

                opts.ResponseType = OpenIdConnectResponseType.Code;

                opts.Scope.Clear();
                opts.Scope.Add("openid");
                opts.Scope.Add("user:read:email");
                opts.Scope.Add("analytics:read:games");
                opts.SaveTokens = true;

                opts.ProtocolValidator.RequireNonce = false;

                opts.GetClaimsFromUserInfoEndpoint = true;

                opts.Events.OnAuthorizationCodeReceived += async (AuthorizationCodeReceivedContext ctx) =>
                {
                    /*var data = new Dictionary<string, string>()
                    {
                        {"client_id",Configuration["Values:Twitch:ClientId"] },
                        {"client_secret",Configuration["Values:Twitch:ClientSecret"]},
                        {"code","" },
                        {"grant_type","authorization_code" },
                        {"redirect_uri","" }
                    };
                    var postData = new FormUrlEncodedContent(data);
                    var httpClient = new HttpClient();
                    var response = await httpClient.PostAsync("", postData);*/
                };
            }
            );
            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
