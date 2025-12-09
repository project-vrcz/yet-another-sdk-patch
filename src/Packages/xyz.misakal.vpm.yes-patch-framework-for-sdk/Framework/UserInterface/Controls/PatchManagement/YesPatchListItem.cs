using UnityEditor;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Controls.PatchManagement;

internal sealed class YesPatchListItem : VisualElement
{
    private readonly Toggle patchToggle;
    private readonly VisualElement patchErrorIcon;

    private readonly Label patchIdLabel;
    private readonly Label patchDisplayNameLabel;
    private readonly Label patchDescriptionLabel;

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
        patchErrorIcon = this.Q<VisualElement>("patch-error-icon");

        patchIdLabel = this.Q<Label>("patch-id");
        patchDisplayNameLabel = this.Q<Label>("patch-display-name");
        patchDescriptionLabel = this.Q<Label>("patch-description");

        patchIdLabel.text = yesPatch.Id;
        patchIdLabel.tooltip = yesPatch.Id;

        patchDisplayNameLabel.text = yesPatch.DisplayName;
        patchDisplayNameLabel.tooltip = yesPatch.DisplayName;

        patchDescriptionLabel.text = yesPatch.Description;
        patchDescriptionLabel.tooltip = yesPatch.Description;

        UpdateUi();

        patchToggle.RegisterValueChangedCallback(enabled =>
        {
            if (enabled.newValue)
                _yesPatchManagerStateManager.EnableAndPatch(_yesPatch.Id);
            else
                _yesPatchManagerStateManager.DisableAndUnPatch(_yesPatch.Id);
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
    }

    private void OnPatchEnabledChanged(object sender, string e)
    {
        if (e != _yesPatch.Id)
            return;

        UpdateUi();
    }

    private void UpdateUi()
    {
        var isErrored = IsPatchErrored();
        patchErrorIcon.style.display = isErrored ? DisplayStyle.Flex : DisplayStyle.None;
        patchToggle.style.display = !isErrored ? DisplayStyle.Flex : DisplayStyle.None;

        patchToggle.SetEnabled(!isErrored);
        patchToggle.SetValueWithoutNotify(_yesPatchManagerStateManager.IsPatchEnabled(_yesPatch.Id));
    }

    private bool IsPatchErrored()
    {
        return _yesPatch.Status is not (
            YesPatchStatus.UnPatched
            or YesPatchStatus.Patched
            or YesPatchStatus.Instantiated);
    }
}