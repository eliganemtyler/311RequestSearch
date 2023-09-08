namespace _311RequestSearch.Server.Entities.Config
{
    public class OmniBar
    {
        private string _baseUriService;
        public string BaseUriService
        {
            // Remapping endpoint to proxy through BFF
            get { return UrlAdjuster.ReplaceTokensAndSetTrailingSlash("{host}{baseHref}/px/omnibar/"); }
            set { this._baseUriService = value; }
        }
    }
}
