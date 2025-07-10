using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEditor.Localization;
using UnityEngine;

using Newtonsoft.Json;

namespace Weblate.Plugin.Server_API.Services
{
    using Weblate.Plugin.ScriptableObjects;
    using Weblate.Plugin.Helpers;
    using Weblate.Plugin.Server_API.Models;

    public class StringTranslationsApiService
    {
        private static WeblateSettings settings = Resources.Load<WeblateSettings>("Weblate/WeblateSettings");

        public static async Task<Dictionary<string, string>> GetComponents()
        {
            string url = $"{settings.Host}/api/projects/{settings.Slug}/components/";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", settings.Token);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            Components components = JsonConvert.DeserializeObject<Components>(json);

            return components.Results.Where(c => c.Slug != "glossary").ToDictionary(c => c.Slug, c => c.Name);
        }

        public static async Task<Dictionary<string, List<(string Lang, Dictionary<string, string> Translations)>>> PullTranslationsAsync(WeblateSettings settings, IEnumerable<string> tableSlugs)
        {
            var result = new Dictionary<string, List<(string Lang, Dictionary<string, string>)>>();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", settings.Token);

            foreach (string slug in tableSlugs)
            {
                var collection = LocalizationEditorSettings.GetStringTableCollections()
                    .FirstOrDefault(c => c.TableCollectionName.ToLower() == slug.ToLower());

                if (collection == null)
                    continue;

                var translations = new List<(string, Dictionary<string, string>)>();

                foreach (var table in collection.StringTables)
                {
                    string langCode = table.LocaleIdentifier.Code;
                    string url = $"{settings.Host}/api/translations/{settings.Slug}/{slug}/{langCode}/file/";

                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                        continue;

                    string fileContent = await response.Content.ReadAsStringAsync();
                    Dictionary<string, string> parsed = FileParser.ParseTranslation(fileContent);
                    translations.Add((langCode, parsed));
                }

                result[slug] = translations;
            }

            return result;
        }
    }
}