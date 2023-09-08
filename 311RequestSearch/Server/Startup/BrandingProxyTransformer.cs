using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;

namespace _311RequestSearch.Server.Startup;

public class BrandingProxyTransformer : HttpTransformer
{
    private readonly string _routeKey;

    public BrandingProxyTransformer(string routeKey) => _routeKey = routeKey;

    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);

        // transform something like /px/branding/Header to /Header (assuming "Header" is the route value specified by _routeKey)
        if (httpContext.Request.RouteValues.TryGetValue(_routeKey, out var routeValue))
            routeValue = $"/{routeValue}";
        else
            routeValue = "/";

        var path = new PathString(routeValue.ToString());

        proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, path, httpContext.Request.QueryString);
    }
}