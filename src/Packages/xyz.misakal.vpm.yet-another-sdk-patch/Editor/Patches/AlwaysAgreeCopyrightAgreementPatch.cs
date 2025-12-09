using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Api;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace YetAnotherPatchForVRChatSdk.Patches;

internal sealed class AlwaysAgreeCopyrightAgreementPatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.yet-another-sdk-patch.always-agree-copyright-agreement";
    public override string DisplayName => "Always Agree Copyright Agreement";
    public override string Description => "Automatically agrees to the copyright agreement when uploading content.";

    public override string Category => "Base SDK Quality of Life Improvements";

    public override bool HasSettingsUi => true;

    public override void CreateSettingsUi(VisualElement rootVisualElement)
    {
        var copyrightAgreementTip = new HelpBox(GetCopyrightAgreementText(), HelpBoxMessageType.Warning)
        {
            style =
            {
                unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
            }
        };

        rootVisualElement.Add(copyrightAgreementTip);
    }

    private static string GetCopyrightAgreementText()
    {
#if YAP4VRC_VRCHAT_BASE_3_8_1_OR_NEWER
        return VRCCopyrightAgreement.AgreementText;
#elif YAP4VRC_VRCHAT_BASE_3_8_0_OR_NEWER
        return
            "By clicking OK, I certify that I have the necessary rights to upload this content and that it will not infringe on any third-party legal or intellectual property rights.";
#endif
    }

#if !YAP4VRC_VRCHAT_BASE_3_8_0_OR_NEWER
    public override void Patch()
    {
    }

    public override void UnPatch()
    {
    }
#else
    private readonly Harmony _harmony = new("xyz.misakal.vpm.yet-another-sdk-patch.always-agree-copyright-agreement");

    private const string HasAgreementMethodName = "HasAgreement";
    private const string AgreeMethodName = "Agree";

    private const string AgreementCode = "content.copyright.owned";
    private const int AgreementVersion = 1;

    private static readonly YesLogger Logger = new(nameof(AlwaysAgreeCopyrightAgreementPatch));

    private static MethodInfo? _agreeMethod;

    public override void Patch()
    {
        var agreeMethod = AccessTools.Method(typeof(VRCCopyrightAgreement), AgreeMethodName, new[] { typeof(string) });
        if (agreeMethod is null || agreeMethod.ReturnType != typeof(Task<bool>))
            throw new MissingMethodException(nameof(VRCCopyrightAgreement), AgreeMethodName);

        _agreeMethod = agreeMethod;

        _harmony.PatchAll(typeof(AlwaysAgreeCopyrightAgreementPatch));
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();
        _agreeMethod = null;
    }

    [HarmonyPatch(typeof(VRCCopyrightAgreement), HasAgreementMethodName, typeof(string))]
    [HarmonyPrefix]
    private static bool HasAgreementPrefix(string contentId, ref Task<bool> __result)
    {
        if (_agreeMethod is null)
        {
            Logger.LogWarning(
                "Agree method is null, cannot auto-agree copyright agreement. Executing original method.");
            return true;
        }

        __result = HasAgreementCore();

        return false;

        async Task<bool> HasAgreementCore()
        {
            if (_agreeMethod is null)
            {
                Logger.LogWarning("Agree method is null, cannot auto-agree copyright agreement.");
                return false;
            }

            var shouldAgree = await ShouldAgreeAsync(contentId);
            if (shouldAgree == ShouldAgreeResult.Error) return false;
            if (shouldAgree == ShouldAgreeResult.AlreadyAgreed) return true;

            try
            {
                var agreeTask = _agreeMethod.Invoke(null, new object[] { contentId }) as Task<bool>;
                if (agreeTask is null)
                {
                    Logger.LogError(
                        $"Failed to invoke Agree method for copyright agreement. (Return is {agreeTask?.GetType().ToString() ?? "null"} not Task<bool>)");
                    return false;
                }

                var result = await agreeTask;
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to auto-agree copyright agreement.");
            }

            return false;
        }
    }

    private static async ValueTask<ShouldAgreeResult> ShouldAgreeAsync(string contentId)
    {
        try
        {
            var result = await VRCApi.CheckContentUploadConsent(new VRCAgreementCheckRequest
            {
                AgreementCode = AgreementCode,
                ContentId = contentId,
                Version = AgreementVersion
            });

            return result.Agreed ? ShouldAgreeResult.AlreadyAgreed : ShouldAgreeResult.ShouldAgree;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check is copyright agreement agreed");
        }

        return ShouldAgreeResult.Error;
    }

    private enum ShouldAgreeResult
    {
        ShouldAgree,
        AlreadyAgreed,
        Error
    }
#endif
}