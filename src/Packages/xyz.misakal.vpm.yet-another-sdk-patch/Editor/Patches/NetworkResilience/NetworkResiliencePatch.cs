using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using BestHTTP;
using HarmonyLib;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.SDKBase.Editor.Api;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;
using YetAnotherPatchForVRChatSdk.Patches.NetworkResilience.SettingsUi;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

[HarmonyPatch]
internal sealed class NetworkResiliencePatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.yet-another-sdk-patch.network-resilience";
    public override string DisplayName => "Network Resilience";
    public override string Description => "Network Resilience Ability for VRChat SDK.";

    public override string Category => "Base SDK Quality of Life Improvements";

    public override bool HasSettingsUi => true;

    private readonly Harmony _harmony = new("xyz.misakal.vpm.yet-another-sdk-patch.network-resilience");

    private static VrcApiHttpClientFactory? _httpClientFactory;
    private static VrcApiHttpClientFactory? _noVrcApiHttpClientFactory;

    private const string GetClientMethodName = "GetClient";

    private const string VrcCookieBaseUrlFieldName = "VRC_COOKIE_BASE_URL";
    private static FieldInfo? _vrcCookieBaseUrlFieldInfo;

    private static readonly NetworkResilienceWebProxy NetworkResilienceWebProxy = new();

    public override void Patch()
    {
        var cookieBaseUrlField = AccessTools.Field(typeof(VRCApi), VrcCookieBaseUrlFieldName);
        if (cookieBaseUrlField is null || !cookieBaseUrlField.IsStatic || cookieBaseUrlField.FieldType != typeof(Uri))
            throw new MissingFieldException(nameof(VRCApi), VrcCookieBaseUrlFieldName);

        _vrcCookieBaseUrlFieldInfo = cookieBaseUrlField;

        _httpClientFactory = null;
        _noVrcApiHttpClientFactory = null;
        CreateOrGetHttpClientFactory();
        CreateOrGetNoVrcApiHttpClientFactory();

        _harmony.PatchAll(typeof(NetworkResiliencePatch));
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();

        _httpClientFactory = null;
        _noVrcApiHttpClientFactory = null;
        _vrcCookieBaseUrlFieldInfo = null;
    }

    private static VrcApiHttpClientFactory CreateOrGetNoVrcApiHttpClientFactory()
    {
        if (_noVrcApiHttpClientFactory is not null)
            return _noVrcApiHttpClientFactory;

        _noVrcApiHttpClientFactory = new VrcApiHttpClientFactory();

        return _noVrcApiHttpClientFactory;
    }

    private static VrcApiHttpClientFactory CreateOrGetHttpClientFactory()
    {
        if (_httpClientFactory is not null)
            return _httpClientFactory;

        _httpClientFactory = new VrcApiHttpClientFactory(SetupAuthCookieContainer);

        return _httpClientFactory;
    }

    private static void SetupAuthCookieContainer(CookieContainer cookieContainer)
    {
        var baseUrl = GetVrcCookieBaseUrl();

        var authCookie = ApiCredentials.GetAuthTokenCookie();
        var twoFactorToken = ApiCredentials.GetTwoFactorAuthTokenCookie();

        if (authCookie is not null)
            cookieContainer.Add(baseUrl, new Cookie(authCookie.Name, authCookie.Value));
        if (twoFactorToken is not null)
            cookieContainer.Add(baseUrl, new Cookie(twoFactorToken.Name, twoFactorToken.Value));
    }

    private static Uri GetVrcCookieBaseUrl()
    {
        if (_vrcCookieBaseUrlFieldInfo is null)
            throw new InvalidOperationException("VRC_COOKIE_BASE_URL field info is not initialized.");

        var uri = (Uri?)_vrcCookieBaseUrlFieldInfo.GetValue(null);
        if (uri is null)
            throw new InvalidOperationException("VRC_COOKIE_BASE_URL is null.");

        return uri;
    }

    [HarmonyPatch(typeof(VRCApi), GetClientMethodName)]
    [HarmonyPrefix]
    private static bool GetClientCore(Uri url, ref HttpClient __result)
    {
        if (url.Host != GetVrcCookieBaseUrl().Host)
        {
            __result = CreateOrGetNoVrcApiHttpClientFactory().GetOrCreateClient();
            return false;
        }

        __result = CreateOrGetHttpClientFactory().GetOrCreateClient();
        return false;
    }

    [HarmonyPatch(typeof(HTTPRequest), nameof(HTTPRequest.Send))]
    [HarmonyPrefix]
    private static void HttpRequestSendPrefix(HTTPRequest __instance)
    {
        var destinationUrl = __instance.CurrentUri;
        if (destinationUrl is null)
            throw new NullReferenceException(nameof(destinationUrl));

        if (__instance.HasProxy || NetworkResilienceWebProxy.IsBypassed(destinationUrl))
            return;

        var proxyUri = NetworkResilienceWebProxy.GetProxy(destinationUrl);
        __instance.Proxy = new HTTPProxy(proxyUri);
    }

    [HarmonyPatch(typeof(HttpClient), nameof(HttpClient.Timeout), MethodType.Setter)]
    [HarmonyPrefix]
    private static bool HttpClientTimeoutSetterPrefix(HttpClient __instance, TimeSpan value)
    {
        // Prevent SDK from setting HttpClient.Timeout
        if (__instance == _httpClientFactory?.GetOrCreateClient() && value != Timeout.InfiniteTimeSpan)
        {
            return false;
        }

        return true;
    }

    public override void CreateSettingsUi(VisualElement rootVisualElement)
    {
        rootVisualElement.Add(new NetworkResiliencePatchSettingsUi());
    }
}