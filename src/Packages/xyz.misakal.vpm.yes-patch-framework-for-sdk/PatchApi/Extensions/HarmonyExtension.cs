using System;
using HarmonyLib;

namespace YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

public static class HarmonyExtension
{
    public static void PatchAll(this Harmony harmony, Type patchType)
    {
        harmony.CreateClassProcessor(patchType).Patch();
    }

    public static void UnpatchSelf(this Harmony harmony)
    {
        harmony.UnpatchAll(harmony.Id);
    }
}