using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace _311RequestSearch.Server.Extensions.Spa
{
  public class SupportedLanguages
  {
    public IReadOnlyDictionary<string, string> LangSpaPathMap { get; private set; }
    public IEnumerable<string> Languages { get; private set; }
    public string DefaultLanguage { get; private set; }

    public SupportedLanguages(IEnumerable<string> languageCodes, string spaRootPath)
    {
      LangSpaPathMap = languageCodes.Select(lang => NormalizeLanguageCode(lang)).ToDictionary(lang => lang, lang => Path.Join(spaRootPath, lang));
      Languages = languageCodes;
      DefaultLanguage = languageCodes.First();
    }

    public static string NormalizeLanguageCode(string code)
    {
      string[] splitCode = code.Split('-');
      if (splitCode.Length > 1)
      {
        splitCode[1] = splitCode[1].ToUpperInvariant();
      }
      splitCode[0] = splitCode[0].ToLowerInvariant();
      return string.Join("-", splitCode);;
    }
  }
}
