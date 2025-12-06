using HarmonyLib;
using UnityEngine;
using VRC.Core;
using YesPatchFrameworkForVRChatSdk.PatchApi;

namespace YetAnotherPatchForVRChatSdk.Patches;

internal sealed class RandomizeDeviceIdPatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.yet-another-sdk-patch.randomize-device-id-patch";
    public override string DisplayName => "Randomize Device ID";
    public override string Description => "Randomizes the device ID sent with API requests.";

    public override string Category => "Privacy?";

    private readonly Harmony _harmony = new("xyz.misakal.vpm.yet-another-sdk-patch.randomize-device-id-patch");

    public override void Patch()
    {
        _harmony.PatchAll(typeof(RandomizeDeviceIdPatch));
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();
    }

    [HarmonyPatch(typeof(API), nameof(API.DeviceID), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool RandomizeDeviceIdPrefix(ref string __result)
    {
        // Generate a random device ID
        __result = System.Guid.NewGuid().ToString();
        return false; // Skip original method
    }
}