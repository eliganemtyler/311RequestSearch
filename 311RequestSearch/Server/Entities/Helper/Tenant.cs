using System;
using System.Linq;
using System.Threading.Tasks;
using _311RequestSearch.Server.Entities.Config;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Tyler.Platform.SDK;
using Tyler.Platform.SDK.Exceptions;

namespace _311RequestSearch.Server.Entities.Helper
{
    public class Tenant : ITenant
    {
        private readonly ClientAppConfig _appConfig;
        private readonly IPlatformSdk _platformSdk;
        private readonly ILogger _logger;

        public Tenant(IConfiguration configuration,
                      IHttpClientFactory httpClientFactory,
                      IHttpContextAccessor httpContextAccessor,
                      ILoggerFactory loggerFactory,
                      IPlatformSdk platformSdk)
        {
            _logger = loggerFactory.CreateLogger("TenantResolver");
            _appConfig = configuration.Get<ClientAppConfig>();
            _platformSdk = platformSdk;
        }

        public async Task<string> ResolveAsync()
        {
            try
            {
                string tenantId = string.Empty;

                var portal = await _platformSdk.GetPortalAsync();

                if (portal != null)
                {
                    tenantId = portal.PortalId;
                }

                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    throw new InvalidOperationException("Could not determine tenantId from url: tenantId cannot be null or empty.");
                }

                return tenantId;
            }
            catch (PlatformSdkApiException ex)
            {
                _logger.LogError(ex, $"Unable to fetch tenant. Error: {ex.Message}. Status: {ex.StatusCode}");
                throw new InvalidOperationException("Could not determine tenantId from url: tenantId cannot be null or empty.");
            }
        }
    }
}
