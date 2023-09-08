namespace _311RequestSearch.Server
{
    public class UrlAdjuster
    {
        public static string ReplaceHostAndBaseHref(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) {
                return input;
            }

            input = input.Replace("{baseHref}", Context.GetBaseHref());

            return input.StartsWith("http") ?
                input.Replace("{host}", Context.GetHost().Replace("https://", "").Replace("http://", "")) :
                input.Replace("{host}", Context.GetHost());
        }

        public static string SetTrailingSlash(string input, bool addSlash = true) 
        {
            if(!string.IsNullOrWhiteSpace(input))
            {
                var trimmed = input.TrimEnd('/');
                return addSlash ? trimmed + "/" : trimmed;
            }
            return input;
        }

        public static string ReplaceTokensAndSetTrailingSlash(string input, bool addSlash = true) {
            return SetTrailingSlash(ReplaceHostAndBaseHref(input), addSlash);
        }
    }
}