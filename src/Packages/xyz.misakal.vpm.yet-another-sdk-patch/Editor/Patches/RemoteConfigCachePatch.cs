using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BestHTTP;
using BestHTTP.JSON;
using HarmonyLib;
using UnityEngine;
using VRC.Core;
using YesPatchFrameworkForVRChatSdk.PatchApi;

namespace YetAnotherPatchForVRChatSdk.Patches;

internal sealed class RemoteConfigCachePatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.remote-config-cache-patch";
    public override string DisplayName => "Remote Config Cache";
    public override string Description => "Cache VRChat Api Config to reduce domain reload times.";
    public override string Category => "Base SDK Network Enhancements";

    private readonly Harmony _harmonyInstance = new("xyz.misakal.vpm.remote-config-cache-patch");

    private const string FetchConfigMethodName = "FetchConfig";

    public override void Patch()
    {
        _harmonyInstance.PatchAll(typeof(RemoteConfigCachePatch));
    }

    public override void UnPatch()
    {
        _harmonyInstance.UnpatchSelf();
    }

    #region Fetche and Cache Api Config

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private static void FetchAndCachedApiConfig(Action<Dictionary<string, object>> onFetched, Action onError)
    {
        var responseContainer = new ApiDictContainer();
        responseContainer.OnSuccess += container =>
        {
            if (container is not ApiDictContainer dictContainer)
            {
                Debug.LogError(
                    "[RemoteConfigCachePatch] Failed to fetch API config: response apiContainer is not ApiDictContainer.");
                onError.Invoke();
                return;
            }

            Debug.Log($"[RemoteConfigCachePatch] Fetched API config for {container}. Caching to disk.");
            var responseDict = dictContainer.ResponseDictionary;
            SaveApiConfigToDisk(responseDict);

            var config = ProcessConfig(responseDict);
            onFetched.Invoke(config);
        };

        responseContainer.OnError += container =>
        {
            Debug.LogError(
                $"[RemoteConfigCachePatch] Failed to fetch API config from server. (code: {container.Code} msg: {container.Text} err: {container.Error})");
            onError.Invoke();
        };

        API.SendRequest(
            "config",
            HTTPMethods.Get,
            responseContainer,
            authenticationRequired: false,
            disableCache: true,
            priority: UpdateDelegator.JobPriority.ApiBlocking);
    }

    #region Disk Cache

    private const string CachedApiConfigFileName = "cached-vrcaht-api-config.json";
    private const string CachedApiConfigTimestampFileName = "cached-vrcaht-api-config-timestamp.txt";

    private static void SaveApiConfigToDisk(IReadOnlyDictionary<string, Json.Token> config)
    {
        var path = GetCachedApiConfigFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var rawConfigJson = ConfigToJson(config);
        File.WriteAllText(path, rawConfigJson);

        var timestampPath = GetCachedApiConfigTimestampFilePath();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        File.WriteAllText(timestampPath, timestamp);

        Debug.Log("[RemoteConfigCachePatch] Saved API config to disk cache.");
    }

    private static IReadOnlyDictionary<string, Json.Token>? GetCachedApiConfigFromDisk()
    {
        var timestamp = GetCachedApiConfigTimestamp();
        if (timestamp == null || DateTimeOffset.UtcNow - timestamp > CacheDuration)
            return null;

        return GetCachedApiConfigFromDiskIgnoreExpiry();
    }

    private static IReadOnlyDictionary<string, Json.Token>? GetCachedApiConfigFromDiskIgnoreExpiry()
    {
        var path = GetCachedApiConfigFilePath();
        if (!File.Exists(path))
            return null;

        var raw = File.ReadAllText(path);
        var config = TryJsonToConfig(raw);
        if (config == null)
            return null;

        Debug.Log("[RemoteConfigCachePatch] Loaded API config from disk cache.");
        return config;
    }

    private static DateTimeOffset? GetCachedApiConfigTimestamp()
    {
        var path = GetCachedApiConfigTimestampFilePath();
        if (!File.Exists(path))
            return null;

        var raw = File.ReadAllText(path);
        if (!long.TryParse(raw, out var timestamp))
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(timestamp);
    }

    private static string GetCachedApiConfigFilePath()
    {
        return Path.Combine(Application.temporaryCachePath, "yap4vrc-sdk-patches", CachedApiConfigFileName);
    }

    private static string GetCachedApiConfigTimestampFilePath()
    {
        return Path.Combine(Application.temporaryCachePath, "yap4vrc-sdk-patches", CachedApiConfigTimestampFileName);
    }

    #endregion

    #region Json

    private static Dictionary<string, object> ProcessConfig(IReadOnlyDictionary<string, Json.Token> config)
    {
        return config.ToDictionary(
            (Func<KeyValuePair<string, Json.Token>, string>)(kv => kv.Key),
            (Func<KeyValuePair<string, Json.Token>, object>)(kv => kv.Value.Value));
    }

    private static string ConfigToJson(IReadOnlyDictionary<string, Json.Token> config)
    {
        return Json.Encode(config, true);
    }

    private static IReadOnlyDictionary<string, Json.Token>? TryJsonToConfig(string json)
    {
        try
        {
            return Json.Decode(json).Object;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #endregion

    #region Apply Config

    private const string ConfigFieldName = "config";

    private static bool TryApplyConfig(BaseConfig configBase, Dictionary<string, object> config)
    {
        var configField = AccessTools.Field(typeof(BaseConfig), ConfigFieldName);
        if (configField == null)
        {
            Debug.LogWarning(
                "[RemoteConfigCachePatch] Failed to apply config: could not find 'config' field.");
            return false;
        }

        configField.SetValue(configBase, config);
        Debug.Log("[RemoteConfigCachePatch] Applied cached API config to RemoteConfig instance.");
        return true;
    }

    #endregion

    [HarmonyPatch(typeof(RemoteConfig), FetchConfigMethodName, typeof(Action), typeof(Action))]
    [HarmonyPrefix]
    private static bool FetchRemoteConfigPrefix(object? __instance, Action? onFetched, Action? onError)
    {
        if (__instance is not RemoteConfig remoteConfig)
        {
            Debug.LogWarning(
                "[RemoteConfigCachePatch] Failed to patch RemoteConfig.FetchConfig: instance is not RemoteConfig. Executing original method.");
            return true;
        }

        // Note: If failed to apply config, execute original method

        // Try to get cached config from disk first
        if (GetCachedApiConfigFromDisk() is { } diskCache)
        {
            var diskConfig = ProcessConfig(diskCache);
            if (!TryApplyConfig(remoteConfig, diskConfig))
            {
                Debug.LogWarning(
                    "[RemoteConfigCachePatch] Failed to apply cached API config. Executing original method.");
                return true;
            }

            onFetched?.Invoke();
            return false;
        }

        // If no valid disk cache, fetch from server
        var isSuccess = false;
        Debug.Log("[RemoteConfigCachePatch] Trying to fetch API config from server...");
        FetchAndCachedApiConfig(fetchedConfig =>
        {
            // Apply fetched config
            if (!TryApplyConfig(remoteConfig, fetchedConfig))
            {
                Debug.LogWarning(
                    "[RemoteConfigCachePatch] Failed to apply fetched API config. Executing original method.");
                isSuccess = false;
                return;
            }

            isSuccess = true;
        }, () =>
        {
            // On error, try to use disk cache ignoring expiry
            Debug.LogWarning("[RemoteConfigCachePatch] Failed to fetch API config. Will use disk cache if available.");
            if (GetCachedApiConfigFromDiskIgnoreExpiry() is not { } diskCacheIgnoreExpiry)
            {
                // No valid disk cache available, report error to caller
                Debug.LogError("[RemoteConfigCachePatch] No valid disk cache available. Report error to caller.");
                isSuccess = false;
                return;
            }

            var diskConfig = ProcessConfig(diskCacheIgnoreExpiry);

            if (!TryApplyConfig(remoteConfig, diskConfig))
            {
                Debug.LogError(
                    "[RemoteConfigCachePatch] Failed to apply cached API config. Report error to caller.");
                isSuccess = false;
                return;
            }

            isSuccess = true;
        });

        if (isSuccess)
            onFetched?.Invoke();
        else
            onError?.Invoke();

        return !isSuccess;
    }
}