using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using _311RequestSearch.Server.Entities.Config;
using _311RequestSearch.Server.Entities.Helper;

namespace _311RequestSearch.Server.Controllers.api
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AppConfigController : BaseController
    {
        private readonly ILogger _logger;
        private readonly ClientAppConfig _appConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenant _tenant;

        public AppConfigController(
                IOptionsSnapshot<ClientAppConfig> appConfig,
                IHttpContextAccessor httpContextAccessor,
                ILoggerFactory loggerFactory, 
                ITenant tenant)
        {
            _logger = loggerFactory.CreateLogger<AppConfigController>();
            _appConfig = appConfig.Value;
            _httpContextAccessor = httpContextAccessor;
            _tenant = tenant;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetAsync()
        {
            try
            {
                // Example of how to resolve tenant
                _appConfig.TenantId = await _tenant.ResolveAsync();
            }
            catch (Exception ex)
            {
                var host = _httpContextAccessor.HttpContext.Request.Host.Value;
                _logger.LogInformation($"Cannot resolve tenant: {host}, message: {ex.Message}");
                _appConfig.TenantId = "cannot resolve tenantid";
            }
            return Ok(_appConfig);
        }
    }
}
