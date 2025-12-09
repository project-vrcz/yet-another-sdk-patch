using System;
using System.Linq;
using UnityEngine;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;
using YesPatchFrameworkForVRChatSdk.PatchManagement;
using YesPatchFrameworkForVRChatSdk.Settings.PatchManager;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

internal sealed class YesPatchManagerStateManager
{
    public static YesPatchManagerStateManager Instance { get; } = new();

    private readonly YesPatchManager _patchManager = YesPatchManager.Instance;
    private readonly YesLogger _logger = new(nameof(YesPatchManagerStateManager));

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
    }

    public void DisablePatchOnly(string patchId)
    {
        var settings = YesPatchManagerSettings.GetOrCreateSettings();
        settings.SetPatchEnabled(patchId, false);
        OnPatchDisabled?.Invoke(this, patchId);

        OnPatchStatusChanged?.Invoke(this, patchId);
    }

    public void EnableAndPatch(string patchId)
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
            _logger.LogError(exception, "Failed to patch: " + patchId);
        }

        var patchEnabled = patch.Status == YesPatchStatus.Patched;
        _patchManager.SetPatchEnabled(patchId, patchEnabled);

        if (patchEnabled)
            OnPatchEnabled?.Invoke(this, patchId);
        else
            OnPatchDisabled?.Invoke(this, patchId);

        OnPatchStatusChanged?.Invoke(this, patchId);
    }

    public void DisableAndUnPatch(string patchId)
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
            _logger.LogError(exception, "Failed to unpatch: " + patchId);
        }

        _patchManager.SetPatchEnabled(patchId, false);

        OnPatchDisabled?.Invoke(this, patchId);
        OnPatchStatusChanged?.Invoke(this, patchId);
    }
}