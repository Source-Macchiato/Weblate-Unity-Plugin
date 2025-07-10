using System.IO;
using UnityEditor;
using UnityEngine;

namespace Weblate.Editor
{
    using Weblate.Editor.UnityLocalization;
    using Weblate.Plugin.ScriptableObjects;

    public class WeblateEditor
    {
        private const string assetFolderPath = "Assets/Resources/Weblate";

        [MenuItem("Tools/Weblate/Pull string translations", false, 90)]
        private static void PullStrings()
        {
            PullStringTranslationsFromWeblate.ShowWindow();
        }

        /*[MenuItem("Tools/Weblate/Push string translations", false, 91)]
        private static void PushStrings()
        {
            PushStringsToWeblate.PushStrings();
        }*/

        [MenuItem("Tools/Weblate/Settings", false, 200)]
        private static void OpenSettings()
        {
            var weblateSettings = LoadSettings();
            Selection.activeObject = weblateSettings;
        }

        private static WeblateSettings LoadSettings()
        {
            var settings = Resources.Load<WeblateSettings>("Weblate/WeblateSettings");

            if (settings == null)
            {
                // Create folder if necessary
                if (!Directory.Exists(assetFolderPath))
                {
                    Directory.CreateDirectory(assetFolderPath);
                    AssetDatabase.Refresh();
                }

                // Create and save asset
                settings = ScriptableObject.CreateInstance<WeblateSettings>();
                AssetDatabase.CreateAsset(settings, $"{assetFolderPath}/WeblateSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return settings;
        }
    }
}
