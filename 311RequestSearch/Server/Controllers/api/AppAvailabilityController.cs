using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Serilog.Context;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Tyler.Platform.SDK;
using Tyler.Platform.SDK.Entity;
using Tyler.Platform.SDK.Exceptions;
using _311RequestSearch.Server.Configuration;
using _311RequestSearch.Server.Entities;
using Microsoft.Extensions.Options;

namespace _311RequestSearch.Server.Controllers.api
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AppAvailabilityController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IPlatformSdk _platformSdk;
        private readonly ServerAppConfig _appConfig;

        public AppAvailabilityController(ILoggerFactory loggerFactory, IOptionsSnapshot<ServerAppConfig> appConfig, IPlatformSdk platformSdk)
        {
            _logger = loggerFactory.CreateLogger<AppAvailabilityController>();
            _appConfig = appConfig.Value;
            _platformSdk = platformSdk;

            LogContext.PushProperty("portalDomain", Context.GetHostServer());
        }

        [HttpGet]
        public async Task<IActionResult> GetAppAvailability()
        {
            try
            {
                var appAvailabilityStatus = await _platformSdk.IsAppAvailableAsync(_appConfig.AppRegistrationId);

                var appAvailability = new AppAvailability
                {
                    IsAppAvailable = appAvailabilityStatus.Status == AvailabilityStatus.Available,
                    RedirectToSignIn = _appConfig.RequireAuthentication && !User.Identity.IsAuthenticated
                };

                return Ok(appAvailability);

            }
            catch (PlatformSdkApiException ex)
            {
                _logger.LogError(ex, $"Unable to determine App Availability Status. Error: {ex.Message}. Status: {ex.StatusCode}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to determine App Availability Status.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to determine App Availability Status. Error: {ex.Message}");
                throw;
            }
        }
    }
}
