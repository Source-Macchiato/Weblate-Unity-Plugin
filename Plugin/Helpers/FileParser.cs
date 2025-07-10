using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Weblate.Plugin.Helpers
{
    using Weblate.Plugin.ScriptableObjects;

    public class FileParser
    {
        private static WeblateSettings settings = Resources.Load<WeblateSettings>("Weblate/WeblateSettings");

        public static Dictionary<string, string> ParseTranslation(string content)
        {
            switch (settings.FileType)
            {
                case WeblateSettings.FileExtensionType.CSV:
                    return ParseCsv(content);

                case WeblateSettings.FileExtensionType.Json:
                    return ParseJson(content);

                case WeblateSettings.FileExtensionType.PO:
                    return ParsePo(content);

                default:
                    return new Dictionary<string, string>();
            }
        }

        // CSV
        private static Dictionary<string, string> ParseCsv(string content)
        {
            var dict = new Dictionary<string, string>();
            using var reader = new StringReader(content);
            bool firstLine = true;
            string line;
            string pendingLine = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (pendingLine != null)
                {
                    line = pendingLine + "\n" + line;
                    pendingLine = null;
                }

                var fields = ParseCsvLine(line);
                if (fields == null)
                {
                    pendingLine = line;
                    continue;
                }

                if (firstLine)
                {
                    firstLine = false;
                    continue;
                }

                if (fields.Count >= 2)
                    dict[fields[0]] = fields[1];
            }

            return dict;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            bool expectingQuote = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                            expectingQuote = true;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Clear();
                        expectingQuote = false;
                    }
                    else if (c == '"' && !expectingQuote)
                    {
                        inQuotes = true;
                    }
                    else if (char.IsWhiteSpace(c) && current.Length == 0)
                    {
                        // skip
                    }
                    else
                    {
                        if (expectingQuote)
                            return null;
                        current.Append(c);
                    }
                }
            }

            if (inQuotes)
                return null;

            fields.Add(current.ToString());
            return fields;
        }

        // Json
        private static Dictionary<string, string> ParseJson(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(content)
                       ?? new Dictionary<string, string>();
            }
            catch (JsonException e)
            {
                Debug.LogError($"[Weblate] Failed to parse JSON: {e.Message}");
                return new Dictionary<string, string>();
            }
        }

        // PO
        private static Dictionary<string, string> ParsePo(string content)
        {
            var dict = new Dictionary<string, string>();
            using var reader = new StringReader(content);

            string line;
            string currentId = null;
            string currentStr = null;
            bool inHeader = true;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // Ignore comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("msgid "))
                {
                    currentId = ParsePoQuote(line.Substring(6).Trim());
                    if (string.IsNullOrEmpty(currentId))
                    {
                        // Empty header, skip
                        inHeader = true;
                        currentId = null;
                        continue;
                    }
                    inHeader = false;
                }
                else if (line.StartsWith("msgstr ") && !inHeader && currentId != null)
                {
                    currentStr = ParsePoQuote(line.Substring(7).Trim());
                    dict[currentId] = currentStr;
                    currentId = null;
                    currentStr = null;
                }
            }

            return dict;
        }

        private static string ParsePoQuote(string input)
        {
            if (input.StartsWith("\"") && input.EndsWith("\""))
                return input.Substring(1, input.Length - 2).Replace("\\n", "\n");
            return input;
        }
    }
}
