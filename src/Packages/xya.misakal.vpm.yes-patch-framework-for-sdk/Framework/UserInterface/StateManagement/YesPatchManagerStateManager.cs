using System;
using System.Linq;
using UnityEngine;
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

    public void EnablePatch(string patchId)
    {
        var patch = _patchManager.Patches.FirstOrDefault(p => p.Id == patchId);
        if (patch == null || (patch.Status != YesPatchStatus.UnPatched && patch.Status != YesPatchStatus.Instantiated))
            return;

        try
        {
            patch.Patch();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Debug.LogError("Failed to enable patch: " + patchId);
        }

        var patchEnabled = patch.Status == YesPatchStatus.Patched;
        _patchManager.SetPatchEnabled(patchId, patchEnabled);

        if (patchEnabled)
            OnPatchEnabled?.Invoke(this, patchId);
        else
            OnPatchDisabled?.Invoke(this, patchId);

        OnPatchStatusChanged?.Invoke(this, patchId);
    }

    public void DisablePatch(string patchId)
    {
        var patch = _patchManager.Patches.FirstOrDefault(p => p.Id == patchId);
        if (patch is not { Status: YesPatchStatus.Patched })
            return;

        try
        {
            patch.Unpatch();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Debug.LogError("Failed to unpatch: " + patchId);
        }

        _patchManager.SetPatchEnabled(patchId, false);

        OnPatchDisabled?.Invoke(this, patchId);
        OnPatchStatusChanged?.Invoke(this, patchId);
    }
}