using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.Settings.PatchManager;

namespace YesPatchFrameworkForVRChatSdk.PatchManagement;

public sealed class YesPatchManager
{
    public static readonly YesPatchManager Instance = new();

    public IReadOnlyList<YesPatch> Patches => _patches.AsReadOnly();
    private readonly List<YesPatch> _patches = new();

    private bool _isPatched;

    public void ApplyPatches()
    {
        if (_isPatched)
            throw new InvalidOperationException("Patches have already been applied.");

        var patches = LoadPatches();
        ApplyPatchesCore(patches);

        // Print summary
        var totalPatches = patches.Length;
        var failedPatches = patches
            .Where(patch => patch.Status != YesPatchStatus.Patched)
            .ToArray();

        var completedMessageBuilder = new StringBuilder();
        completedMessageBuilder.AppendLine(
            $"[YesPatchFramework] Patch process completed. Total: {totalPatches} Errors: {failedPatches.Length}");

        if (failedPatches.Length > 0)
        {
            completedMessageBuilder.AppendLine("The following patches failed to apply:");
            foreach (var failedPatch in failedPatches)
            {
                completedMessageBuilder.AppendLine($"- [{failedPatch.Id}] {failedPatch.DisplayName}");
            }
        }

        Debug.Log(completedMessageBuilder.ToString());
    }

    public void SetPatchEnabled(string patchId, bool enabled)
    {
        var settings = YesPatchManagerSettings.GetOrCreateSettings();

        settings.SetPatchEnabled(patchId, enabled);
    }

    private void ApplyPatchesCore(YesPatch[] patches)
    {
        var settings = YesPatchManagerSettings.GetOrCreateSettings();

        foreach (var patch in patches)
        {
            if (!settings.IsPatchEnabled(patch.Id))
                continue;

            Debug.Log($"[YesPatchFramework] Applying patch: [{patch.Id}] {patch.DisplayName}");
            try
            {
                patch.Patch();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[YesPatchFramework] Failed to apply patch: [{patch.Id}] {patch.DisplayName}");
                Debug.LogException(ex);
            }
        }

        _isPatched = true;
    }

    private YesPatch[] LoadPatches()
    {
        if (_patches.Count != 0)
            throw new InvalidOperationException("Patches have already been loaded.");

        var patchTypes = GetExportedPatchTypes();

        foreach (var patchType in patchTypes)
        {
            try
            {
                var patchInstance = (YesPatchBase)Activator.CreateInstance(patchType);
                _patches.Add(new YesPatch(patchInstance, patchType));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[YesPatchFramework] Failed to create patch instance for type: {patchType.FullName}");
                Debug.LogException(ex);
            }
        }

        var duplicateIds = _patches
            .GroupBy(patch => patch.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            var duplicateIdsString = string.Join(", ", duplicateIds);
            throw new Exception($"Duplicate patch ids found: {duplicateIdsString}");
        }

        return _patches.ToArray();
    }

    private static Type[] GetExportedPatchTypes()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var exportPatchTypes = assemblies
            .SelectMany(assembly => assembly.GetCustomAttributes())
            .Where(attribute => attribute is ExportYesPatchAttribute)
            .OfType<ExportYesPatchAttribute>()
            .Select(attribute => attribute.ExportType)
            .ToArray();

        return exportPatchTypes;
    }
}