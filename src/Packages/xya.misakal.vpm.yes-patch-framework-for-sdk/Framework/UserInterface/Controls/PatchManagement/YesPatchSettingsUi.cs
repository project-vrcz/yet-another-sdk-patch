using System;
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

    private readonly TextField _patchErrorMessageTextField;
    private readonly VisualElement _patchErrorTip;

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

        _patchErrorMessageTextField = this.Q<TextField>("patch-error-message");
        _patchErrorTip = this.Q<VisualElement>("patch-error-tip");

        UpdateUiStatus();

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
            _yesPatchManagerStateManager.OnPatchStatusChanged += OnPatchEnabledChanged;
        });

        RegisterCallback<DetachFromPanelEvent>(_ =>
        {
            _yesPatchManagerStateManager.OnPatchEnabled -= OnPatchEnabledChanged;
            _yesPatchManagerStateManager.OnPatchDisabled -= OnPatchEnabledChanged;
            _yesPatchManagerStateManager.OnPatchStatusChanged -= OnPatchEnabledChanged;
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

        UpdateUiStatus();
    }

    private void UpdateUiStatus()
    {
        var isErrored = IsPatchErrored();

        _patchErrorTip.style.display = isErrored ? DisplayStyle.Flex : DisplayStyle.None;

        _patchErrorMessageTextField.style.display = isErrored ? DisplayStyle.Flex : DisplayStyle.None;
        _patchErrorMessageTextField.value = _patch.Status switch
        {
            YesPatchStatus.PatchFailed => _patch.LastPatchException?.ToString() ??
                                          "Patch failed. But no exception was recorded.",
            YesPatchStatus.UnPatchFailed => _patch.LastUnpatchException?.ToString() ??
                                            "Unpatch failed. But no exception was recorded.",
            _ =>
                "You should not see this message. Please report a bug. It means patch status is ok but error message is shown."
        };

        _patchEnabledToggle.style.display = !isErrored ? DisplayStyle.Flex : DisplayStyle.None;
        _patchEnabledToggle.SetEnabled(!isErrored);
        _patchEnabledToggle.SetValueWithoutNotify(_yesPatchManagerStateManager.IsPatchEnabled(_patch.Id));
    }

    private bool IsPatchErrored()
    {
        return _patch.Status is not (
            YesPatchStatus.UnPatched
            or YesPatchStatus.Patched
            or YesPatchStatus.Instantiated);
    }
}