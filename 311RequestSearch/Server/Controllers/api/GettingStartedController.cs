using System;
using System.Threading.Tasks;
using _311RequestSearch.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using _311RequestSearch.Server.Entities.Helper;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace _311RequestSearch.Server.Controllers.api
{
    [Route("api/[controller]")]
    [Authorize]
    //[AllowAnonymous]
    public class GettingStartedController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenant _tenant;

        public GettingStartedController(ILoggerFactory loggerFactory,
                                        IConfiguration configuration,
                                        IHttpClientFactory httpClientFactory,
                                        IHttpContextAccessor httpContextAccessor,
                                        ITenant tenant)
        {
            _logger = loggerFactory.CreateLogger<GettingStartedController>();
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _httpContextAccessor = httpContextAccessor;
            _tenant = tenant;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GettingStarted), 200)]
        public async Task<IActionResult> Get()
        {
            try
            {
                //not used in GettingStarted, but here for demo purposes
                string tenantId = await _tenant.ResolveAsync();

                return await Task.FromResult(Ok(new GettingStarted { ApplicationName = "311RequestSearch" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, "{message}", "Unable to get status");
                return BadRequest();
            }
        }
    }
}