using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using _311RequestSearch.Server.Entities.Config;

namespace _311RequestSearch.Server
{
    public static class Context
    {
        private static IHttpContextAccessor HttpContextAccessor;
        private static string _baseHref;
        private static string _requestScheme;
        public static void Configure(IHttpContextAccessor httpContextAccessor, string baseHref, IConfiguration configuration)
        {
            HttpContextAccessor = httpContextAccessor;
            _baseHref = baseHref;
            _requestScheme = configuration.GetValue<string>("EnvironmentDomainProtocol");
            if (string.IsNullOrWhiteSpace(_requestScheme))
                _requestScheme = "https"; 
        }

        public static string GetBaseHref() {
            return _baseHref;
        }

        public static Uri GetAbsoluteUri()
        {
            var request = HttpContextAccessor.HttpContext.Request;
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = _requestScheme; 
            uriBuilder.Host = request.Host.Host;
            if (request.Host.Port.HasValue)
            {
                uriBuilder.Port = request.Host.Port.Value;
            }
            uriBuilder.Path = request.Path;
            uriBuilder.Query = request.QueryString.ToUriComponent();
            return uriBuilder.Uri;
        }

        public static string GetHost()
        {
            var uri = GetAbsoluteUri();

            // remove default ports
            if ((uri.Scheme == "http" && uri.Port == 80) ||
                (uri.Scheme == "https" && uri.Port == 443))
            {
              return uri.Scheme + "://" + uri.Host;  
            } 

            return uri.Scheme + "://" + uri.Host + ":" + uri.Port;
        }

        public static string GetHostServer()
        {
            var uri = GetAbsoluteUri();
            if ((uri.Port != 80) && (uri.Port != 443))
                return uri.Host + ":" + uri.Port;
            return uri.Host;
        }        
        
        public static string GetAbsoluteUrl() { return GetAbsoluteUri().AbsoluteUri; }
        public static string GetAbsolutePath() { return GetAbsoluteUri().AbsolutePath; }
    }
}