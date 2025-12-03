using System;
using YesPatchFrameworkForVRChatSdk.Exceptions.Patch;
using YesPatchFrameworkForVRChatSdk.PatchApi;

namespace YesPatchFrameworkForVRChatSdk.PatchManagement;

public sealed class YesPatch
{
    public Type PatchType { get; }

    public string Id => _instance.Id;
    public string DisplayName => _instance.DisplayName;
    public bool IsDefaultEnabled => _instance.IsDefaultEnabled;

    public YesPatchStatus Status { get; private set; } = YesPatchStatus.Instantiated;
    public Exception? LastPatchException { get; private set; }
    public Exception? LastUnpatchException { get; private set; }

    private readonly YesPatchBase _instance;
    private bool _isPatched;

    internal YesPatch(YesPatchBase patchInstance, Type patchType)
    {
        _instance = patchInstance;
        PatchType = patchType;
    }

    public void Patch()
    {
        if (_instance is null)
            throw new InvalidOperationException("The patch has not been instantiated.");

        if (_isPatched)
            throw new InvalidOperationException("The patch has already applied.");

        if (Status != YesPatchStatus.Instantiated && Status != YesPatchStatus.UnPatched)
            throw new InvalidOperationException(
                "The patch is not in a valid state (Instantiated or UnPatched) to be applied.");

        try
        {
            _instance.Patch();
        }
        catch (Exception ex)
        {
            LastPatchException = ex;
            Status = YesPatchStatus.PatchFailed;

            throw new YesPatchApplyFailedException(ex);
        }

        _isPatched = true;
        Status = YesPatchStatus.Patched;
    }

    public void Unpatch()
    {
        if (_instance is null)
            throw new InvalidOperationException("The patch has not been instantiated.");

        if (!_isPatched)
            throw new InvalidOperationException("The patch has not been applied.");

        if (Status != YesPatchStatus.Patched)
            throw new InvalidOperationException("The patch is not in a valid state (Patched) to be applied.");

        try
        {
            _instance.UnPatch();
        }
        catch (Exception ex)
        {
            LastUnpatchException = ex;
            Status = YesPatchStatus.UnPatchFailed;

            throw new YesPatchUnPatchFailedException(ex);
        }

        _isPatched = false;
        Status = YesPatchStatus.UnPatched;
    }
}

public enum YesPatchStatus
{
    Instantiated,
    PatchFailed,
    Patched,
    UnPatchFailed,
    UnPatched
}