using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

// DO NOT USE file scoped namespace, It will brake ScriptableObject creation
namespace YesPatchFrameworkForVRChatSdk.Settings.PatchManager
{
    internal sealed class YesPatchManagerSettings : ScriptableObject
    {
        public const string FolderName = "YesPatchFrameworkForVRChatSdk";
        public const string FolderPath = "Assets/" + FolderName;
        public const string AssetPath = "Assets/" + FolderName + "/" + "YesPatchManagerSettings.asset";

        [SerializeField] private List<YesPatchManagerPatchSettings> patchSettings = new();

        [SerializeField] public YesLogLevel unityConsoleMinLogLevel = YesLogLevel.Info;

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

        public bool IsPatchEnabled(string patchId, bool fallbackValue = false)
        {
            var settingIndex = patchSettings.FindIndex(s => s.id == patchId);
            if (settingIndex == -1)
                return fallbackValue;

            var setting = patchSettings[settingIndex];
            return setting.isEnabled;
        }

        public void SetPatchEnabled(string patchId, bool isEnabled)
        {
            var settingIndex = patchSettings.FindIndex(s => s.id == patchId);
            var setting = settingIndex == -1 ? new YesPatchManagerPatchSettings() : patchSettings[settingIndex];

            setting.id = patchId;
            setting.isEnabled = isEnabled;

            if (settingIndex == -1)
            {
                patchSettings.Add(new YesPatchManagerPatchSettings
                {
                    id = patchId,
                    isEnabled = isEnabled
                });
            }
            else
            {
                patchSettings[settingIndex] = setting;
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}