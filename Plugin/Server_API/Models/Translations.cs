using Newtonsoft.Json;

namespace Weblate.Plugin.Server_API.Models
{
    // /api/components/(string:project)/(string:component)/translations/
    public class Translations
    {
        [JsonProperty("results")]
        public Translations[] Results { get; set; }
    }

    public class TranslationsResults
    {
        [JsonProperty("language_code")]
        public string LanguageCode { get; set; }
    }
}