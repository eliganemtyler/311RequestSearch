using System.Collections.Generic;
using System.Linq;
using static Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults;

namespace _311RequestSearch.Server.Startup
{
    public static class DefaultMimeTypes
    {
        public static IEnumerable<string> Get => MimeTypes.Concat(new[]
        {
            "image/svg+xml",
            "application/font-woff2"
        });
    }

}