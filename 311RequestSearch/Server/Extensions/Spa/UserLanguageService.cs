using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Linq;
using Microsoft.Extensions.Primitives;
using System;

namespace _311RequestSearch.Server.Extensions.Spa
{
  public class UserLanguageService : IUserLanguageService
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SupportedLanguages _supportedLanguages;

    public UserLanguageService(IHttpContextAccessor httpContextAccessor, SupportedLanguages supportedLanguages)
    {
      _httpContextAccessor = httpContextAccessor;
      _supportedLanguages = supportedLanguages;
    }

    public string GetUserLocale()
    {
      var request = _httpContextAccessor.HttpContext?.Request;
      if (request == null)
      {
        return _supportedLanguages.DefaultLanguage;
      }

      var cookieLocale = request.Cookies[".User.Locale"];
      if (!string.IsNullOrWhiteSpace(cookieLocale))
      {
        return SupportedLanguages.NormalizeLanguageCode(cookieLocale);
      }

      var userLocales = request.Headers["Accept-Language"].ToString();
      var userAcceptLanguage = GetAcceptLanguageFromHeaderOrNull(userLocales);

      if (!string.IsNullOrWhiteSpace(userAcceptLanguage))
      {
        return SupportedLanguages.NormalizeLanguageCode(userAcceptLanguage);
      }

      return _supportedLanguages.DefaultLanguage;
    }

    public string GetAcceptLanguageFromHeaderOrNull(string headerValue)
    {
      if (headerValue == null)
      {
        return null;
      }
      try
      {
        var clientLanguages = (headerValue)
          .Split(',')
          .Select(StringWithQualityHeaderValue.Parse)
          .OrderByDescending(language => language.Quality.GetValueOrDefault(1))
          .Select(language => language.Value)
          .Distinct()
          .Where(languageCode => !string.IsNullOrWhiteSpace(languageCode) && languageCode.Trim() != "*");
        return clientLanguages
          .FirstOrDefault(clientLanguage => _supportedLanguages.Languages.Contains(clientLanguage, StringComparer.InvariantCultureIgnoreCase));
      }
      catch
      {
        return null;
      }
    }
  }
}
