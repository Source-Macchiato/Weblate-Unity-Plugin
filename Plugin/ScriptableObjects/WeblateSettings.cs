using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace Weblate.Plugin.ScriptableObjects
{
    [CreateAssetMenu(fileName = "WeblateSettings", menuName = "Weblate/Settings", order = 0)]
    public class WeblateSettings : ScriptableObject
    {
        [SerializeField, ReadOnly, Tooltip("The url of the host. For exemple https://hosted.weblate.org")]
        private string host;

        [SerializeField, ReadOnly, Tooltip("A user token (starting with wlu_) or a project token (starting with wlp_). A project token is recommended for security.")]
        private string token;

        [SerializeField, ReadOnly, Tooltip("The slug of the project.")]
        private string slug;

        [HideInInspector]
        public bool allSelected = true;

        [HideInInspector]
        public List<string> selectedTables = new();

        public enum FileExtensionType
        {
            CSV,
            Json,
            PO
        }

        [SerializeField, ReadOnly, Tooltip("")]
        private FileExtensionType fileType = FileExtensionType.Json;

        public string Host
        {
            get => host;
            set => host = value;
        }

        public string Token
        {
            get => token;
            set => token = value;
        }

        public string Slug
        {
            get => slug;
            set => slug = value;
        }

        public FileExtensionType FileType
        {
            get => fileType;
            set => fileType = value;
        }
    }
}