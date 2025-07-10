using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Weblate.Editor.UnityLocalization
{
    public class PushStringsToWeblate
    {
        private static IReadOnlyCollection<StringTableCollection> stringTableCollection;

        public static void PushStrings()
        {
            stringTableCollection = LocalizationEditorSettings.GetStringTableCollections();

            foreach (StringTableCollection table in stringTableCollection)
            {
                Debug.Log(table.TableCollectionName);
            }
        }
    }
}