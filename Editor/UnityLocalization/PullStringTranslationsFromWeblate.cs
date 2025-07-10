using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Weblate.Editor.UnityLocalization
{
    using Weblate.Plugin.Server_API.Services;
    using Weblate.Plugin.ScriptableObjects;

    public class PullStringTranslationsFromWeblate : EditorWindow
    {
        private static WeblateSettings settings;
        private static string status;
        private static bool isPulling;

        private int currentTableTypeIndex;
        private Vector2 scrollPosition;
        private Dictionary<string, string> tables;
        private Dictionary<string, bool> tableStates = new();

        public static void ShowWindow()
        {
            settings = Resources.Load<WeblateSettings>("Weblate/WeblateSettings");
            if (settings == null)
            {
                Debug.LogError("WeblateSettings.asset not found in Resources/Weblate");
                return;
            }
            status = "Ready.";
            isPulling = false;

            GetWindow(typeof(PullStringTranslationsFromWeblate), true, "Pull string translations from Weblate");
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;

            tables = LocalizationEditorSettings.GetStringTableCollections().Select(collection => collection.TableCollectionName).ToDictionary(table => table.ToLower(), table => table);

            currentTableTypeIndex = settings.allSelected ? 0 : 1;

            foreach (string selectedTable in settings.selectedTables)
            {
                tableStates[selectedTable] = true;
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnGUI()
        {
            if (isPulling)
            {
                GUILayout.Label(status);
                return;
            }

            if (tables != null) // Default popup
            {
                currentTableTypeIndex = GUILayout.SelectionGrid(currentTableTypeIndex, new[]
                {
                    " All tables",
                    " Selected tables"
                }, 1, "radio");

                if (currentTableTypeIndex == 1)
                {
                    foreach (var table in tables)
                    {
                        bool currentState = tableStates.TryGetValue(table.Key, out bool state) && state;

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(28);

                        tableStates[table.Key] = GUILayout.Toggle(currentState, table.Value);

                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Pull string translations from Weblate"))
                {
                    if (currentTableTypeIndex == 1 && !tableStates.Values.Any(v => v))
                    {
                        status = "No table selected.";
                    }
                    else
                    {
                        isPulling = true;
                        status = "Starting pull...";
                        PullTranslations();
                    }
                }

                GUILayout.Space(10);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GUILayout.Label($"Status: {status}", EditorStyles.boldLabel);
                GUILayout.EndScrollView();
            }
            else // If an error occurred while fetching components
            {
                GUILayout.Label(status);
            }
        }

        private async void PullTranslations()
        {
            try
            {
                isPulling = true;
                status = "Pulling translations...";

                var selectedSlugs = tables
                    .Where(kvp => currentTableTypeIndex == 0 || (tableStates.TryGetValue(kvp.Key, out var selected) && selected))
                    .Select(kvp => kvp.Key)
                    .ToList();

                var result = await StringTranslationsApiService.PullTranslationsAsync(settings, selectedSlugs);

                foreach (var (slug, translationsPerLang) in result)
                {
                    string tableName = tables[slug];
                    var collection = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(c => c.TableCollectionName == tableName);
                    var sharedData = collection.SharedData;

                    foreach (var (lang, translations) in translationsPerLang)
                    {
                        var stringTable = collection.StringTables.FirstOrDefault(t => t.LocaleIdentifier.Code == lang);
                        if (stringTable == null)
                        {
                            status += $"\nMissing table for language {lang} in '{tableName}'";
                            continue;
                        }

                        int imported = 0;

                        foreach (var kv in translations)
                        {
                            var entry = sharedData.GetEntry(kv.Key) ?? sharedData.AddKey(kv.Key);
                            var valueEntry = stringTable.GetEntry(entry.Id) ?? stringTable.AddEntry(entry.Id, kv.Value);
                            valueEntry.Value = kv.Value;
                            imported++;
                        }

                        EditorUtility.SetDirty(stringTable);
                        status += $"\nImported {imported} into '{tableName}' [{lang}]";
                    }

                    EditorUtility.SetDirty(collection);
                    EditorUtility.SetDirty(sharedData);
                }
            }
            catch (Exception ex)
            {
                status += $"\nError: {ex.Message}";
            }
            finally
            {
                isPulling = false;
            }
        }

        [Serializable]
        private class TranslationWrapper
        {
            public List<TranslationEntry> data;
            public Dictionary<string, string> ToDict()
            {
                var dict = new Dictionary<string, string>();
                foreach (var e in data)
                {
                    dict[e.key] = e.translation;
                }
                return dict;
            }
        }

        [Serializable]
        private class TranslationEntry
        {
            public string key;
            public string translation;
        }

        private void OnEditorUpdate()
        {
            Repaint();
        }
    }
}