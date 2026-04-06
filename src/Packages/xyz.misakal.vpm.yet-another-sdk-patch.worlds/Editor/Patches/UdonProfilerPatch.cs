using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Unity.Profiling;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;
using UnityObject = UnityEngine.Object;

namespace YetAnotherPatchForVRChatSdk.Worlds.Patches;

[HarmonyPatch]
internal sealed class UdonProfilerPatch : YesPatchBase
{
    public override string Id => "xyz.misakal.vpm.yet-another-sdk-patch.worlds.udon-profiler";
    public override string DisplayName => "Udon Profiler";
    public override string Category => "Worlds SDK Enhancements";

    public override string Description =>
        "Add more detail markers to Unity Profiler for Udon behaviours execution.";

    public override bool HasSettingsUi => true;

    private readonly Harmony _harmony = new("xyz.misakal.vpm.yet-another-sdk-patch.worlds.udon-profiler");

    private static int _mainThreadId;

    [RuntimeInitializeOnLoadMethod]
    private static void OnInitialized()
    {
        _mainThreadId = Environment.CurrentManagedThreadId;
    }

    private static bool IsMainThread()
    {
        return _mainThreadId == Environment.CurrentManagedThreadId;
    }

    public override void Patch()
    {
        ProfilerMarkerScope.CleanMarkerCache();
        _harmony.PatchAll(typeof(UdonProfilerPatch));
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();
        ProfilerMarkerScope.CleanMarkerCache();
    }

    private struct PatchState
    {
        public ProfilerMarker Marker;
        public bool InMainThread;
    }

    [HarmonyPatch(typeof(UdonBehaviour), nameof(UdonBehaviour.RunProgram), typeof(uint))]
    [HarmonyPrefix]
    private static void UdonBehaviourRunProgramPrefix(
        uint entryPoint,
        UdonBehaviour __instance,
        ref IUdonProgram ____program,
        out PatchState __state)
    {
        if (IsMainThread())
        {
            __state = new PatchState
            {
                Marker = ProfilerMarkerScope.GetProfilerMarker(__instance, ____program, entryPoint),
                InMainThread = true
            };
            __state.Marker.Begin(__instance.gameObject);
        }
        else
        {
            __state = new PatchState
            {
                InMainThread = false
            };
        }
    }

    [HarmonyPatch(typeof(UdonBehaviour), nameof(UdonBehaviour.RunProgram), typeof(uint))]
    [HarmonyFinalizer]
    private static void Finalizer(ref PatchState __state)
    {
        if (__state.InMainThread)
        {
            __state.Marker.End();
        }
    }

    private readonly struct ProfilerMarkerScope : IDisposable
    {
        private readonly ProfilerMarker _marker;

        private ProfilerMarkerScope(ProfilerMarker profilerMarker, UnityObject obj)
        {
            _marker = profilerMarker;
            _marker.Begin(obj);
        }

        public void Dispose()
        {
            _marker.End();
        }

        private static readonly Dictionary<(int, uint), ProfilerMarker> ProfilerMarkerEventCache = new();

        [PublicAPI]
        public static void CleanMarkerCache()
        {
            ProfilerMarkerEventCache.Clear();
        }

        [PublicAPI]
        public static ProfilerMarker GetProfilerMarker(
            UdonBehaviour udonBehaviour, IUdonProgram udonProgram, uint entryPoint)
        {
            var programID = udonBehaviour.programSource.GetInstanceID();
            var key = (programID, entryPoint);

            if (!ProfilerMarkerEventCache.TryGetValue(key, out var profilerMarker))
            {
                var programName = udonBehaviour.programSource.name;
                if (!udonProgram.EntryPoints.TryGetSymbolFromAddress(entryPoint, out var symbolName))
                {
                    symbolName = "[UNKNOWN]";
                }

                profilerMarker = new ProfilerMarker($"Udon {programName}.{symbolName}");
                ProfilerMarkerEventCache[key] = profilerMarker;
            }

            return profilerMarker;
        }

        [PublicAPI]
        public static ProfilerMarkerScope Auto(UdonBehaviour udonBehaviour, IUdonProgram udonProgram, uint entryPoint)
        {
            return new ProfilerMarkerScope
            (
                GetProfilerMarker(udonBehaviour, udonProgram, entryPoint), udonBehaviour.gameObject
            );
        }
    }
}