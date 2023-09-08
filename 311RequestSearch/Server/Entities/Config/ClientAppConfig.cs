namespace _311RequestSearch.Server.Entities.Config
{
    // Properties defined in this class will be made available to the client app via the /AppConfig endpoint
    public class ClientAppConfig
    {
        public string Title { get; set; }
        public string DefaultLanguage { get; set; }

        public string Version { get; set; }
        public string NotFoundPageUri { get; set; }
        public string ViewProfileUri { get; set; }

        private string _brandingServiceUri;
        public string BrandingServiceUri
        {
            // Remapping endpoint to proxy through BFF
            get { return UrlAdjuster.ReplaceTokensAndSetTrailingSlash("{host}{baseHref}/px/branding/"); }
            set { this._brandingServiceUri = value; }
        }
        public string TenantId { get; set; }
        public DatadogRum DatadogRum { get; set; }
        public OneTrustConfig OneTrustConfig { get; set; }
    }
}
