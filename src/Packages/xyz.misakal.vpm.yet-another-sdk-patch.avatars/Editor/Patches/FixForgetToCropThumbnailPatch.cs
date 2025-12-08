using System;
using System.Reflection;
using HarmonyLib;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Api;
using YesPatchFrameworkForVRChatSdk.PatchApi;

namespace YetAnotherPatchForVRChatSdk.Avatars.Patches;

internal sealed class FixForgetToCropThumbnailPatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.yet-another-sdk-patch.avatars.fix-forget-to-crop-thumbnail";
    public override string DisplayName => "Fix .sb Forget to Crop Thumbnail";
    public override string Category => "Avatars SDK bugs fixes";

    public override string Description =>
        "Fix the issue that VRChat SDK forgets to crop the avatar thumbnail when creating new avatar in SDK 3.9.0 to 3.10.0. (Fixed by VRChat in SDK 3.10.1)";

#if YAP4VRC_VRCHAT_AVATARS_3_9_0_OR_NEWER && !YAP4VRC_VRCHAT_AVATARS_3_10_1_OR_NEWER
    private readonly Harmony _harmony =
        new("xyz.misakal.vpm.yet-another-sdk-patch.avatars.fix-forget-to-crop-thumbnail");

    private const string CropImageMethodName = "CropImage";

    private static MethodInfo? _cropImageMethodInfo;

    public override void Patch()
    {
        // internal static string CropImage
        // (string sourcePath, float width, float height, bool centerCrop = true, bool forceLinear = false, bool forceGamma = false)
        var corpImageMethod = AccessTools.Method(typeof(VRC_EditorTools), CropImageMethodName,
            new[] { typeof(string), typeof(float), typeof(float), typeof(bool), typeof(bool), typeof(bool) });

        if (corpImageMethod is null || corpImageMethod.ReturnType != typeof(string))
            throw new MissingMethodException(nameof(VRC_EditorTools), CropImageMethodName);

        _cropImageMethodInfo = corpImageMethod;

        _harmony.PatchAll(typeof(FixForgetToCropThumbnailPatch));
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();
        _cropImageMethodInfo = null;
    }

    [HarmonyPatch(typeof(VRCApi), nameof(VRCApi.CreateNewAvatar))]
    [HarmonyPrefix]
    private static void ApiCreateNewAvatarPrefix(ref string? pathToImage)
    {
        if (pathToImage == null)
            return;

        pathToImage = CropThumbnail(pathToImage);
    }

    private static string CropThumbnail(string pathToImage)
    {
        if (_cropImageMethodInfo == null)
            throw new MissingMethodException(nameof(VRC_EditorTools), CropImageMethodName);

        var croppedImagePath = _cropImageMethodInfo.Invoke(null,
            new object[] { pathToImage, 800f, 600f, true, false, false }) as string;

        if (croppedImagePath == null)
            throw new Exception("VRC_EditorTools.CropImage() returned null");

        return croppedImagePath;
    }
#else
    public override void Patch()
    {
    }

    public override void UnPatch()
    {
    }
#endif
}