using UnityEditor;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Controls.PatchManagement;

internal sealed class YesPatchSettingsUi : VisualElement
{
    private readonly YesPatchManagerStateManager _yesPatchManagerStateManager = YesPatchManagerStateManager.Instance;
    private readonly YesPatch _patch;

    private readonly Label _patchDisplayNameLabel;
    private readonly Label _patchDescriptionLabel;

    private readonly VisualElement _customUiContainer;

    private readonly Toggle _patchEnabledToggle;

    public YesPatchSettingsUi(YesPatch patch)
    {
        _patch = patch;

        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/xyz.misakal.vpm.yes-patch-framework-for-sdk/Framework/UserInterface/Controls/PatchManagement/" +
            nameof(YesPatchSettingsUi) + ".uxml");
        tree.CloneTree(this);

        _patchDisplayNameLabel = this.Q<Label>("patch-settings-header-display-name");
        _patchDescriptionLabel = this.Q<Label>("patch-settings-header-description");

        _customUiContainer = this.Q<VisualElement>("patch-settings-custom-ui-container");

        _patchEnabledToggle = this.Q<Toggle>("patch-settings-enabled-toggle");

        _patchDisplayNameLabel.text = _patch.DisplayName;
        _patchDisplayNameLabel.tooltip = _patch.DisplayName;

        _patchDescriptionLabel.text = _patch.Description;
        _patchDescriptionLabel.tooltip = _patch.Description;

        UpdateToggle();

        _patchEnabledToggle.RegisterValueChangedCallback(enabled =>
        {
            if (enabled.newValue)
                _yesPatchManagerStateManager.EnablePatch(_patch.Id);
            else
                _yesPatchManagerStateManager.DisablePatch(_patch.Id);
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

        if (!patch.HasSettingsUi)
        {
            return;
        }

        _customUiContainer.Clear();

        patch.CreateSettingsUi(_customUiContainer);
        _customUiContainer.Add(new IMGUIContainer(patch.OnSettingUi));
    }

    private void OnPatchEnabledChanged(object sender, string e)
    {
        if (e != _patch.Id)
            return;

        UpdateToggle();
    }

    private void UpdateToggle()
    {
        _patchEnabledToggle.SetValueWithoutNotify(_yesPatchManagerStateManager.IsPatchEnabled(_patch.Id));
    }
}