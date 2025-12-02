using UnityEditor;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Controls.PatchManagement;

internal sealed class YesPatchListItem : VisualElement
{
    private readonly Toggle patchToggle;

    private readonly Label patchIdLabel;
    private readonly Label patchDisplayNameLabel;

    private readonly YesPatch _yesPatch;
    private readonly YesPatchManagerStateManager _yesPatchManagerStateManager;

    public YesPatchListItem(YesPatch yesPatch, YesPatchManagerStateManager yesPatchManager)
    {
        _yesPatch = yesPatch;
        _yesPatchManagerStateManager = yesPatchManager;

        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/xyz.misakal.vpm.yes-patch-framework-for-sdk/Framework/UserInterface/Controls/PatchManagement/" +
            nameof(YesPatchListItem) + ".uxml");
        tree.CloneTree(this);

        patchToggle = this.Q<Toggle>("patch-toggle");

        patchIdLabel = this.Q<Label>("patch-id");
        patchDisplayNameLabel = this.Q<Label>("patch-display-name");

        patchIdLabel.text = yesPatch.Id;
        patchDisplayNameLabel.text = yesPatch.DisplayName;

        UpdateToggle();

        patchToggle.RegisterValueChangedCallback(enabled =>
        {
            if (enabled.newValue)
                _yesPatchManagerStateManager.EnablePatch(_yesPatch.Id);
            else
                _yesPatchManagerStateManager.DisablePatch(_yesPatch.Id);
        });

        RegisterCallback<AttachToPanelEvent>(_ =>
        {
            _yesPatchManagerStateManager.OnPatchEnabled += OnPatchEnabledChanged;
            _yesPatchManagerStateManager.OnPatchDisabled += OnPatchEnabledChanged;
        });

        RegisterCallback<DetachFromPanelEvent>(_ =>
        {
            _yesPatchManagerStateManager.OnPatchEnabled -= OnPatchEnabledChanged;
            _yesPatchManagerStateManager.OnPatchDisabled -= OnPatchEnabledChanged;
        });
    }

    private void OnPatchEnabledChanged(object sender, string e)
    {
        if (e != _yesPatch.Id)
            return;

        UpdateToggle();
    }

    private void UpdateToggle()
    {
        patchToggle.SetValueWithoutNotify(_yesPatchManagerStateManager.IsPatchEnabled(_yesPatch.Id));
    }
}