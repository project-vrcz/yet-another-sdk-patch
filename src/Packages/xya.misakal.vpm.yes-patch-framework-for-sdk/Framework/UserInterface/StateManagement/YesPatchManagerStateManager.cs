using System;
using System.Linq;
using YesPatchFrameworkForVRChatSdk.PatchManagement;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

internal sealed class YesPatchManagerStateManager
{
    public static YesPatchManagerStateManager Instance { get; } = new();

    private readonly YesPatchManager _patchManager = YesPatchManager.Instance;

    public event EventHandler<string>? OnPatchEnabled;
    public event EventHandler<string>? OnPatchDisabled;

    public bool IsPatchEnabled(string patchId)
    {
        var patch = _patchManager.Patches
            .FirstOrDefault(p => p.Id == patchId);

        return patch is { Status: YesPatchStatus.Patched };
    }

    public void EnablePatch(string patchId)
    {
        var patch = _patchManager.Patches.FirstOrDefault(p => p.Id == patchId);
        if (patch == null || (patch.Status != YesPatchStatus.UnPatched && patch.Status != YesPatchStatus.Instantiated))
            return;

        patch.Patch();
        _patchManager.SetPatchEnabled(patchId, true);
        OnPatchEnabled?.Invoke(this, patchId);
    }

    public void DisablePatch(string patchId)
    {
        var patch = _patchManager.Patches.FirstOrDefault(p => p.Id == patchId);
        if (patch is not { Status: YesPatchStatus.Patched })
            return;

        patch.Unpatch();
        _patchManager.SetPatchEnabled(patchId, false);
        OnPatchDisabled?.Invoke(this, patchId);
    }
}