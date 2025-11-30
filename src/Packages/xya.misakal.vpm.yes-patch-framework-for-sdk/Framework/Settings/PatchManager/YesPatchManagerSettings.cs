using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// DO NOT USE file scoped namespace, It will brake ScriptableObject creation
namespace YesPatchFrameworkForVRChatSdk.Settings.PatchManager
{
    internal sealed class YesPatchManagerSettings : ScriptableObject
    {
        public const string FolderName = "YesPatchFrameworkForVRChatSdk";
        public const string FolderPath = "Assets/" + FolderName;
        public const string AssetPath = "Assets/" + FolderName + "/" + "YesPatchManagerSettings.asset";

        [SerializeField] private List<YesPatchManagerPatchSettings> patchSettings = new();

        internal static YesPatchManagerSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<YesPatchManagerSettings>(AssetPath);
            if (settings != null)
                return settings;

            settings = CreateInstance<YesPatchManagerSettings>();

            if (!AssetDatabase.IsValidFolder(FolderPath))
                AssetDatabase.CreateFolder("Assets", FolderName);

            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        public bool IsPatchEnabled(string patchId)
        {
            var setting = patchSettings.Find(s => s.Id == patchId);
            return setting?.IsEnabled ?? true;
        }

        public void SetPatchEnabled(string patchId, bool isEnabled)
        {
            var setting = patchSettings.Find(s => s.Id == patchId);
            if (setting != null)
            {
                setting.IsEnabled = isEnabled;
            }
            else
            {
                patchSettings.Add(new YesPatchManagerPatchSettings(patchId, isEnabled));
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}