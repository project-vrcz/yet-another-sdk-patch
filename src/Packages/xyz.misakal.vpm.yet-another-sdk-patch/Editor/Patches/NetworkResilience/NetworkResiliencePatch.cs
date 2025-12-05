using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using HarmonyLib;
using VRC.Core;
using VRC.SDKBase.Editor.Api;
using YesPatchFrameworkForVRChatSdk.PatchApi;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

internal sealed class NetworkResiliencePatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.yet-another-sdk-patch.network-resilience";
    public override string DisplayName => "Network Resilience";
    public override string Description => "Network Resilience Ability for VRChat SDK.";

    public override string Category => "Base SDK Quality of Life Improvements";

    private readonly Harmony _harmony = new("xyz.misakal.vpm.yet-another-sdk-patch.network-resilience");

    private const string GetClientMethodName = "GetClient";

    private static VrcApiHttpClientFactory? _httpClientFactory;
    private static VrcApiHttpClientFactory? _noVrcApiHttpClientFactory;

    private const string VrcCookieBaseUrlFieldName = "VRC_COOKIE_BASE_URL";
    private static FieldInfo? _vrcCookieBaseUrlFieldInfo;

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
}