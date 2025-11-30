using System;

namespace YesPatchFrameworkForVRChatSdk.Settings.PatchManager;

[Serializable]
internal sealed class YesPatchManagerPatchSettings
{
    public YesPatchManagerPatchSettings(string id, bool isEnabled)
    {
        Id = id;
        IsEnabled = isEnabled;
    }

    public string Id { get; set; }
    public bool IsEnabled { get; set; }
}