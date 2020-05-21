using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;
using TwitchChatBot.Shared.Services;
using TwitchChatBot.Web.v2.Areas.Identity;
using TwitchChatBot.Web.v2.Data;
using TwitchChatBot.Web.v2.Hubs;

namespace TwitchChatBot.Web.v2
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            services.AddSingleton<WeatherForecastService>();

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddSingleton<ITwitchService, TwitchService>();
            services.AddSingleton<IStorageService, AzureTableStorageService>();

            services.AddSignalR();
                /*impl => 
            {
                var httpClientFactory = (IHttpClientFactory)impl.GetService(typeof(IHttpClientFactory));
                var logger = (ILogger<TwitchService>)impl.GetService(typeof(ILogger<TwitchService>));

                var svc = new TwitchService(httpClientFactory, logger);
                var accessor= (IHttpContextAccessor)impl.GetService(typeof(IHttpContextAccessor));
                var context = accessor.HttpContext;
                var accessToken = context.GetTokenAsync("access_token").Result;
                var refreshToken = context.GetTokenAsync("refresh_token").Result;
                
                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
                {
                    svc.SetTokens(accessToken, refreshToken).Wait();
                }
                return svc;
            });*/

            services.AddAuthentication(opts =>
            {
                opts.DefaultChallengeScheme = "Twitch";
            })
            .AddOpenIdConnect("Twitch", opts =>
            {
                opts.Authority = "https://id.twitch.tv/oauth2";
                opts.ClientId = Configuration[Constants.CONFIG_TWITCH_CLIENTID];
                opts.ClientSecret = Configuration[Constants.CONFIG_TWITCH_CLIENTSECRET];

                opts.ResponseType = OpenIdConnectResponseType.Code;

                opts.Scope.Clear();
                opts.Scope.Add("openid");
                opts.Scope.Add("user:read:email");
                opts.Scope.Add("analytics:read:games");
                opts.SaveTokens = true;

                opts.GetClaimsFromUserInfoEndpoint = true;

                opts.Events = new OpenIdConnectEvents
                {
                    OnTokenResponseReceived = async ctx => 
                    {
                        var httpClientFactory = (IHttpClientFactory)ctx.HttpContext.RequestServices.GetService(typeof(IHttpClientFactory));
                        var httpClient = httpClientFactory.CreateClient();

                        var data = new Dictionary<string, string>
                        {
                            {"client_id", Configuration[Constants.CONFIG_TWITCH_CLIENTID] },
                            {"client_secret",Configuration[Constants.CONFIG_TWITCH_CLIENTSECRET] },
                            {"grant_type","client_credentials" },
                            {"scope","" }
                        };
                        var response = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(data));
                        response.EnsureSuccessStatusCode();

                        var responseData = await response.Content.ReadAsStringAsync();
                        var json = JsonSerializer.Deserialize<TwitchClientCredentialsResponse>(responseData);


                        var twitchService =(ITwitchService)ctx.HttpContext.RequestServices.GetService(typeof(ITwitchService));
                        await twitchService.SetTokens(ctx.TokenEndpointResponse.AccessToken, ctx.TokenEndpointResponse.RefreshToken,json.AccessToken, Configuration[Constants.CONFIG_TWITCH_CLIENTID]);
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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
                endpoints.MapHub<TwitchHub>("/twitchhub");
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
