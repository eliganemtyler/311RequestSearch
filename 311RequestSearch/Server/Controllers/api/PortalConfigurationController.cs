using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using _311RequestSearch.Server.Entities.Config;
using _311RequestSearch.Server.Entities.Helper;
using Tyler.Platform.SDK;
using Tyler.Platform.SDK.Models;

namespace _311RequestSearch.Server.Controllers.api
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PortalConfigurationController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IPlatformSdk _platformSDK;
        private readonly ITenant _tenant;
        private readonly IHttpContextAccessor _contextAccessor;

        public PortalConfigurationController(
            ILoggerFactory loggerFactory,
            ITenant tenant,
            IPlatformSdk platformSdk,
            IHttpContextAccessor contextAccessor)
        {
            _logger = loggerFactory.CreateLogger<PortalConfigurationController>();
            _platformSDK = platformSdk;
            _tenant = tenant;
            LogContext.PushProperty("portalDomain", Context.GetHostServer());
            _contextAccessor = contextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> GetPortalConfiguration()
        {
            try
            {
                var portalInfo = await _platformSDK.GetPortalAsync();
                var portalConfiguration = new PortalConfig
                {
                    FooterLinks = BuildFooterLinks(portalInfo)
                };

                return Ok(portalConfiguration);
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to get Portal Configuration.", e.Message);
                throw;
            }
        }

        private List<FooterLink> BuildFooterLinks(PortalInfo portalInfo)
        {
            var footerLinks = new List<FooterLink>();

            if (!String.IsNullOrEmpty(portalInfo.AgencyHomePageUrl))
            {
                var homeName = portalInfo.AgencyHomePageTitle ?? "Home page";
                footerLinks.Add(new FooterLink(homeName, portalInfo.AgencyHomePageUrl));
            }

            if (!String.IsNullOrEmpty(portalInfo.AgencyContactUrl))
            {
                footerLinks.Add(new FooterLink("Contact", portalInfo.AgencyContactUrl));
            }

            if (!String.IsNullOrEmpty(portalInfo.AgencyTermsOfUseUrl))
            {
                footerLinks.Add(new FooterLink("Terms of use", portalInfo.AgencyTermsOfUseUrl));
            }

            if (!String.IsNullOrEmpty(portalInfo.AgencyPrivacyPolicyUrl))
            {
                footerLinks.Add(new FooterLink("Privacy policy", portalInfo.AgencyPrivacyPolicyUrl));
            }

            return footerLinks;
        }
    }
}
