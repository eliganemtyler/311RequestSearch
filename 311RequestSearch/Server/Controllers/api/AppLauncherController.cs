using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Serilog.Context;
using Tyler.Platform.SDK;
using Tyler.Platform.DynamicAuth.Extensions;
using tcp_omni_service_sdk;
using _311RequestSearch.Server.Entities.Helper;

namespace _311RequestSearch.Server.Controllers.api
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AppLauncherController : BaseController
    {
        private readonly ILogger _logger;
        private IConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly IPlatformSdk _platformSdk;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IntentsClient _platformOmniSdk;
        private readonly ITenant _tenant;

        public AppLauncherController(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IPlatformSdk platformSdk,
            ITenant tenant)
        {
            _logger = loggerFactory.CreateLogger<AppLauncherController>();
            LogContext.PushProperty("portalDomain", Context.GetHostServer());
            _configuration = configuration;
            _client = httpClientFactory.CreateClient();
            _platformSdk = platformSdk;
            _httpContextAccessor = httpContextAccessor;
            _platformOmniSdk = new IntentsClient(_configuration.GetValue<string>("OmniBar:baseUri"), _client);
            _tenant = tenant;
        }

        [HttpGet]
        [Route("{*url}")]
        public async Task<IActionResult> GetIntents()
        {
            try
            {
                var sub = string.Empty;
                var tenantId = await _tenant.ResolveAsync();

                if (User.Identity.IsAuthenticated)
                    sub = User.Identity.GetSub();
                var intents = await _platformOmniSdk.GetCitizenIntentsByCategoryAsync("Launcher", tenantId, sub);

                return Ok(intents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace, $"Unable to fetch applauncher intents. Error: {ex.Message}");
                throw;
            }
        }
    }
}
