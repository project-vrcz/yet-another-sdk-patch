using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience.SettingsUi;

internal sealed class NetworkResiliencePatchSettingsUi : VisualElement
{
    private readonly NetworkResiliencePatchSettings _settings = NetworkResiliencePatchSettings.GetOrCreateSettings();

    private readonly VisualElement _customProxyUriValidationMessage;
    private readonly TextField _customProxyUriField;
    private readonly EnumField _proxyModeField;

    private const string VisualTreeAssetGuid = "cdd9f4145c19b564b967e698981ab473";

    public NetworkResiliencePatchSettingsUi()
    {
        var path = AssetDatabase.GUIDToAssetPath(VisualTreeAssetGuid);
        if (string.IsNullOrEmpty(path))
            throw new FileNotFoundException("Could not find VisualTreeAsset for NetworkResiliencePatchSettingsUi.");

        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        tree.CloneTree(this);

        _proxyModeField = this.Q<EnumField>("proxy-mode");
        _customProxyUriField = this.Q<TextField>("proxy-uri");
        _customProxyUriValidationMessage = this.Q<VisualElement>("custom-proxy-uri-valid-error");

        _proxyModeField.Init(ProxyMode.NoProxy);

        UpdateUi();

        _proxyModeField.RegisterValueChangedCallback(evt =>
        {
            _settings.ProxyMode = (ProxyMode)evt.newValue;
            _settings.Save();
            UpdateUi();
        });

        _customProxyUriField.RegisterValueChangedCallback(evt =>
        {
            _settings.CustomProxyAddress = evt.newValue;
            _settings.Save();
            UpdateUi();
        });
    }

    private void UpdateUi()
    {
        _proxyModeField.SetValueWithoutNotify(_settings.ProxyMode);
        _customProxyUriField.SetValueWithoutNotify(_settings.CustomProxyAddress);

        var isCustomProxySelected = _settings.ProxyMode == ProxyMode.CustomProxy;

        _customProxyUriField.style.display = new StyleEnum<DisplayStyle>(
            isCustomProxySelected ? DisplayStyle.Flex : DisplayStyle.None
        );

        if (!isCustomProxySelected)
        {
            _customProxyUriValidationMessage.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            return;
        }

        var isCustomUriValid = _settings.TryGetProxyUri(out _);
        _customProxyUriValidationMessage.style.display = new StyleEnum<DisplayStyle>(
            isCustomUriValid ? DisplayStyle.None : DisplayStyle.Flex
        );
    }
}