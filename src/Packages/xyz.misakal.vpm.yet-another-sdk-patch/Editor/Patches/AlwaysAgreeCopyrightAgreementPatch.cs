using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Api;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace YetAnotherPatchForVRChatSdk.Patches;

[HarmonyPatch]
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
    private const string ContentListKey = "VRCSdkControlPanel.CopyrightAgreement.ContentList";

    private const string AgreementCode = "content.copyright.owned";
    private const int AgreementVersion = 1;

    private static readonly YesLogger Logger = new(nameof(AlwaysAgreeCopyrightAgreementPatch));

    public override void Patch()
    {
        _harmony.PatchAll(typeof(AlwaysAgreeCopyrightAgreementPatch));
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();
    }

    #region Agree Implmeation (Api request & Session storage)

    private static async Task AgreeAsync(string contentId)
    {
        var result = await OriginalContentUploadConsent(new VRCAgreement
        {
            AgreementCode = AgreementCode,
            AgreementFulltext = GetCopyrightAgreementText(),
            ContentId = contentId,
            Version = AgreementVersion
        });

        var agreed = result.ContentId == contentId &&
                     result is { Version: AgreementVersion, AgreementCode: AgreementCode };
        if (!agreed)
        {
            throw new Exception(
                "Failed to agree copyright agreement due to api response mismatch with request agreement content");
        }

        SaveContentAgreementToSession(contentId);
    }

    private static void SaveContentAgreementToSession(string contentId)
    {
        var agreedList = GetAgreedContentThisSession();
        if (agreedList.Contains(contentId)) return;

        agreedList.Add(contentId);
        SessionState.SetString(ContentListKey, string.Join(";", agreedList));
    }

    private static List<string> GetAgreedContentThisSession()
    {
        var saved = SessionState.GetString(ContentListKey, null);
        if (string.IsNullOrWhiteSpace(saved))
        {
            return new List<string>();
        }

        return saved.Split(';').ToList();
    }

    [HarmonyPatch(typeof(VRCApi), nameof(VRCApi.ContentUploadConsent), typeof(VRCAgreement))]
    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static Task<VRCAgreement> OriginalContentUploadConsent(VRCAgreement data)
    {
        // Should never be called, just a placeholder for reverse patch
        throw new NotSupportedException("This method is a Harmony reverse patch and should never be called directly.");
    }

    #endregion

    [HarmonyPatch(typeof(VRCApi), nameof(VRCApi.ContentUploadConsent), typeof(VRCAgreement))]
    [HarmonyPrefix]
    private static bool ContentUploadConsentPrefix(VRCAgreement data, ref Task<VRCAgreement> __result)
    {
        __result = AgreeAndReturnResultAsync();
        return false;

        async Task<VRCAgreement> AgreeAndReturnResultAsync()
        {
            await AgreeAsync(data.ContentId);
            return data;
        }
    }

    [HarmonyPatch(typeof(VRCCopyrightAgreement), HasAgreementMethodName, typeof(string))]
    [HarmonyPrefix]
    private static bool HasAgreementPrefix(string contentId, ref Task<bool> __result)
    {
        __result = CheckAndAgreeAsync(contentId);

        return false;
    }

    private static async Task<bool> CheckAndAgreeAsync(string contentId)
    {
        var shouldAgree = await ShouldAgreeAsync(contentId);
        if (shouldAgree == ShouldAgreeResult.Error) return false;
        if (shouldAgree == ShouldAgreeResult.AlreadyAgreed) return true;

        try
        {
            await AgreeAsync(contentId);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to auto-agree copyright agreement.");
        }

        return false;
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