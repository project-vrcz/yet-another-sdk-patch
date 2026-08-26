using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

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
        CachedProfilerMarker.ClearCache();
        _harmony.PatchAll(typeof(UdonProfilerPatch));
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    public override void UnPatch()
    {
        _harmony.UnpatchSelf();
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        CachedProfilerMarker.ClearCache();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode or PlayModeStateChange.EnteredEditMode:
                CachedProfilerMarker.ClearCache();
                break;
        }
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
                Marker = CachedProfilerMarker.Get(__instance, ____program, entryPoint),
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

    private readonly struct CachedProfilerMarker
    {
        private static readonly Dictionary<(int, uint), ProfilerMarker> Cache = new();
        private static readonly ProfilerMarker MarkerMissingSource = new("Udon [Missing Source]");
        private static readonly ProfilerMarker MarkerMissingProgram = new("Udon [Missing Program]");

        [PublicAPI]
        public static void ClearCache()
        {
            Cache.Clear();
        }

        [PublicAPI]
        public static ProfilerMarker Get(UdonBehaviour udonBehaviour, IUdonProgram? udonProgram, uint entryPoint)
        {
            var programSource = udonBehaviour.programSource;
            if (programSource == null)
            {
                return MarkerMissingSource;
            }

            if (udonProgram == null)
            {
                return MarkerMissingProgram;
            }

            var programID = programSource.GetInstanceID(); // TODO Unity 6 support
            var key = (programID, entryPoint);

            if (Cache.TryGetValue(key, out var profilerMarker))
            {
                return profilerMarker;
            }

            // Slow path if cache miss
            if (!udonProgram.EntryPoints.TryGetSymbolFromAddress(entryPoint, out var symbolName))
            {
                symbolName = "[UNKNOWN]";
            }

            profilerMarker = new ProfilerMarker($"Udon {programSource.name}.{symbolName}");
            Cache[key] = profilerMarker;
            return profilerMarker;
        }
    }
}