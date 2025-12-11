using System;
using System.IO;
using AmplitudeSDKWrapper;
using HarmonyLib;
using UnityEngine;
using VRC.Core;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace YetAnotherPatchForVRChatSdk.Patches;

[HarmonyPatch]
internal sealed class NoTelemetryPatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.yet-another-sdk-patch.no-telemetry";
    public override string DisplayName => "Disable SDK Telemetry";
    public override string Description => "Disables all telemetry and analytics in the VRChat SDK.";
    public override string Category => "Privacy?";

    private static readonly YesLogger Logger = new(nameof(NoTelemetryPatch));

    private readonly Harmony _harmony = new("xyz.misakal.vpm.yet-another-sdk-patch.no-telemetry");

    public override void Patch()
    {
        _harmony.PatchAll(typeof(NoTelemetryPatch));
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();
    }

    [HarmonyPatch(typeof(AnalyticsInterface), nameof(AnalyticsInterface.Send))]
    [HarmonyPrefix]
    private static bool SendEventPrefix()
    {
        TryDeleteAmplitudeCache();
        return false;
    }

    [HarmonyPatch(typeof(AmplitudeWrapper), "UpdateServer")]
    [HarmonyPrefix]
    private static bool UpdateServerPrefix()
    {
        TryDeleteAmplitudeCache();
        return false;
    }

    private static void TryDeleteAmplitudeCache()
    {
        var cachePaths = GetAmplitudeCachePaths();
        foreach (var cachePath in cachePaths)
        {
            if (!File.Exists(cachePath))
                continue;

            Logger.LogDebug("[NoTelemetryPatch] Deleting Amplitude cache file: " + cachePath);
            try
            {
                File.Delete(cachePath);
            }
            catch (Exception e)
            {
                Logger.LogWarning("[NoTelemetryPatch] Failed to delete Amplitude cache file: " + cachePath + "\n" + e);
            }
        }
    }

    private static string[] GetAmplitudeCachePaths()
    {
        return new[]
        {
            Path.Combine(Application.temporaryCachePath, "amplitude.cache"),
            Path.Combine(Application.temporaryCachePath, "settings_com.amplitude")
        };
    }
}