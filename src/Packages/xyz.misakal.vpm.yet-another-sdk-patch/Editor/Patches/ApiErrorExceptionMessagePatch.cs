using System;
using System.Net.Http;
using System.Reflection;
using HarmonyLib;
using VRC.SDKBase.Editor.Api;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

namespace YetAnotherPatchForVRChatSdk.Patches;

[HarmonyPatch]
internal sealed class ApiErrorExceptionMessagePatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.yet-another-sdk-patch.api-error-exception-message";
    public override string DisplayName => "Add meaningful message for ApiErrorException";
    public override string Description => "Replace default exception message of ApiErrorException";

    public override string Category => "Base SDK Quality of Life Improvements";

    private readonly Harmony _harmony = new("xyz.misakal.vpm.yet-another-sdk-patch.api-error-exception-message");

    private static FieldInfo? _exceptionMessageField;

    public override void Patch()
    {
        _exceptionMessageField ??=
            typeof(Exception).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
        if (_exceptionMessageField is null)
            throw new MissingFieldException("_message field is missing from Exception type");

        _harmony.PatchAll(typeof(ApiErrorExceptionMessagePatch));
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ApiErrorException), MethodType.Constructor, typeof(HttpResponseMessage), typeof(string))]
    [HarmonyPostfix]
    private static void ApiErrorExceptionConstructorPostfix(ApiErrorException __instance)
    {
        if (_exceptionMessageField is null)
            return;

        var request = __instance.HttpMessage.RequestMessage;
        var message =
            $"{request.Method} {request.RequestUri} failed with status code {(int)__instance.StatusCode}: {__instance.ErrorMessage}";

        _exceptionMessageField.SetValue(__instance, message);
    }
}