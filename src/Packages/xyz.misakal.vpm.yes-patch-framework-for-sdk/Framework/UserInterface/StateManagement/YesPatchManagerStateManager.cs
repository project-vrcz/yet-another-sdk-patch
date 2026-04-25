using System;
using System.Linq;
using UnityEditor.Compilation;
using YesPatchFrameworkForVRChatSdk.PatchManagement;
using YesPatchFrameworkForVRChatSdk.Settings.PatchManager;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

internal sealed class YesPatchManagerStateManager
{
    public static YesPatchManagerStateManager Instance { get; } = new();

    private readonly YesPatchManager _patchManager = YesPatchManager.Instance;

    public event EventHandler<string>? OnPatchEnabled;
    public event EventHandler<string>? OnPatchDisabled;

    public event EventHandler<string>? OnPatchStatusChanged;

    public bool IsPatchEnabled(string patchId)
    {
        if (_patchManager.Patches.FirstOrDefault(p => p.Id == patchId) is not { } patch)
            return false;

        var settings = YesPatchManagerSettings.GetOrCreateSettings();
        return settings.IsPatchEnabled(patchId, patch.IsDefaultEnabled);
    }

    public void EnablePatchOnly(string patchId)
    {
        var settings = YesPatchManagerSettings.GetOrCreateSettings();
        settings.SetPatchEnabled(patchId, true);
        OnPatchEnabled?.Invoke(this, patchId);
        OnPatchStatusChanged?.Invoke(this, patchId);

        CompilationPipeline.RequestScriptCompilation();
    }

    public void DisablePatchOnly(string patchId)
    {
        var settings = YesPatchManagerSettings.GetOrCreateSettings();
        settings.SetPatchEnabled(patchId, false);
        OnPatchDisabled?.Invoke(this, patchId);
        OnPatchStatusChanged?.Invoke(this, patchId);

        CompilationPipeline.RequestScriptCompilation();
    }

    public void EnableAndPatch(string patchId) => EnablePatchOnly(patchId);

    public void DisableAndUnPatch(string patchId) => DisablePatchOnly(patchId);
}