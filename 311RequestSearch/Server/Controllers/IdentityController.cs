using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using _311RequestSearch.Server.Controllers.api;
using _311RequestSearch.Server.Entities;
using Tyler.Platform.DynamicAuth;
using Microsoft.Extensions.Options;
using _311RequestSearch.Server.Entities.Config;
using _311RequestSearch.Server.Extensions;
using Tyler.Platform.DynamicAuth.Extensions;
using System.Globalization;

namespace _311RequestSearch.Server.Controllers
{
    public class IdentityController : Controller
    {
        private readonly ILogger _logger;

        public IdentityController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<IdentityController>();
        }


        [AllowAnonymous]
        [HttpGet, Route("userinfo")]
        public IActionResult GetUserInfo()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userInfo = User.Identity.GetUserInfo();
            return Ok(userInfo);
        }

        // This exectutes after the signin flow has completed and the user is authenticated
        [Authorize]
        [HttpGet, Route("signin")]
        public IActionResult Signin([FromQuery] string redirectUrl="")
        {
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                if (redirectUrl.StartsWith(baseUrl, ignoreCase: true, culture: CultureInfo.InvariantCulture))
                {
                    return Redirect(redirectUrl);
                }
            }
            return Redirect(Request.PathBase);
        }

        [HttpGet, Route("signout")]
        public IActionResult Signout()
        {
            var schemes = new List<string> { "cookie" };
            var currentScheme = User.Claims.FirstOrDefault(c => c.Type == TcpAuthenticationConstants.AuthSchemeClaimType)?.Value;
            if (!string.IsNullOrEmpty(currentScheme))
                schemes.Add(currentScheme);

            return base.SignOut(schemes.ToArray());
        }

    }


}
