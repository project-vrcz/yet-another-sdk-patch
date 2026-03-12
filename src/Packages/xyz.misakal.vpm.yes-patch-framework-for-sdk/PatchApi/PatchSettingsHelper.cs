using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace YesPatchFrameworkForVRChatSdk.PatchApi;

public static class PatchSettingsHelper
{
    private const string FolderName = "YesPatchFrameworkForVRChatSdk";
    private const string FolderPath = "ProjectSettings/" + FolderName;

    public static string CreateSettingsFolderIfNotExists(string patchSettingsId)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        if (patchSettingsId.Any(c => invalidChars.Contains(c)))
            throw new ArgumentException("The patchSettingsId contains invalid characters.", nameof(patchSettingsId));

        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);

        var settingsFolderPath = Path.Combine(FolderPath, patchSettingsId);
        if (!Directory.Exists(settingsFolderPath))
            Directory.CreateDirectory(settingsFolderPath);

        return settingsFolderPath;
    }

    public static string CreateSettingsFolderIfNotExists(string patchSettingsId, string settingsFileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        if (settingsFileName.Any(c => invalidChars.Contains(c)))
            throw new ArgumentException("The settingsFileName contains invalid characters.", nameof(patchSettingsId));

        return CreateSettingsFolderIfNotExists(patchSettingsId) + "/" + settingsFileName;
    }

    public static T GetOrCreateSettingsJson<T>(string patchSettingsId, string settingsFileName,
        Func<T> createdSettingsAction)
    {
        var settingsPath = CreateSettingsFolderIfNotExists(patchSettingsId, settingsFileName);

        if (File.Exists(settingsPath))
        {
            try
            {
                var settingsRaw = File.ReadAllText(settingsPath);
                return JsonConvert.DeserializeObject<T>(settingsRaw) ??
                       throw new Exception("Settings json is null JSON");
            }
            catch (Exception ex)
            {
                YesLogger.LogError(
                    ex,
                    nameof(T),
                    "Failed to deserialize patch settings.",
                    null
                );
            }
        }

        var settings = createdSettingsAction();
        File.WriteAllText(settingsPath, JsonConvert.SerializeObject(settings));
        return settings;
    }

    public static void SaveSettingsJson<T>(string patchSettingsId, string settingsFileName, T settings)
    {
        var settingsPath = CreateSettingsFolderIfNotExists(patchSettingsId, settingsFileName);
        File.WriteAllText(settingsPath, JsonConvert.SerializeObject(settings));
    }
}