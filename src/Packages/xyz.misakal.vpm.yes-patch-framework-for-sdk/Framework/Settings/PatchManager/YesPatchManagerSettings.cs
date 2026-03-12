using System.Collections.Generic;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace YesPatchFrameworkForVRChatSdk.Settings.PatchManager;

internal sealed class YesPatchManagerSettings
{
    private const string SettingsId = "xyz.misakal.vpm.yes-patch-framework-for-sdk";
    private const string SettingsFileName = "patch-manager.json";

    public List<YesPatchManagerPatchSettings> PatchSettings { get; } = new();
    public YesLogLevel UnityConsoleMinLogLevel { get; set; } = YesLogLevel.Info;

    private static YesPatchManagerSettings? _instance;

    internal static YesPatchManagerSettings GetOrCreateSettings()
    {
        if (_instance is not null)
            return _instance;

        _instance = PatchSettingsHelper.GetOrCreateSettingsJson(
            SettingsId,
            SettingsFileName,
            () => new YesPatchManagerSettings()
        );

        _instance.Save();

        return _instance;
    }

    public bool IsPatchEnabled(string patchId, bool fallbackValue = false)
    {
        var settingIndex = PatchSettings.FindIndex(s => s.Id == patchId);
        if (settingIndex == -1)
            return fallbackValue;

        var setting = PatchSettings[settingIndex];
        return setting.IsEnabled;
    }

    public void SetPatchEnabled(string patchId, bool isEnabled)
    {
        var settingIndex = PatchSettings.FindIndex(s => s.Id == patchId);
        var setting = settingIndex == -1 ? new YesPatchManagerPatchSettings() : PatchSettings[settingIndex];

        setting.Id = patchId;
        setting.IsEnabled = isEnabled;

        if (settingIndex == -1)
        {
            PatchSettings.Add(new YesPatchManagerPatchSettings
            {
                Id = patchId,
                IsEnabled = isEnabled
            });
        }
        else
        {
            PatchSettings[settingIndex] = setting;
        }

        Save();
    }

    public void Save()
    {
        PatchSettingsHelper.SaveSettingsJson(SettingsId, SettingsFileName, this);
    }
}