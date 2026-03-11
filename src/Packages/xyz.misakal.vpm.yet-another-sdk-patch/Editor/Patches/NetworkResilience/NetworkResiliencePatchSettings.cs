using System;
using YesPatchFrameworkForVRChatSdk.PatchApi;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience
{
    internal sealed class NetworkResiliencePatchSettings
    {
        private const string SettingsId = "xyz.misakal.vpm.yet-another-sdk-patch.network-resilience";
        private const string SettingsFileName = nameof(NetworkResiliencePatchSettings) + ".json";

        private static NetworkResiliencePatchSettings? _instance;

        internal static NetworkResiliencePatchSettings GetOrCreateSettings()
        {
            if (_instance is not null)
                return _instance;

            _instance = PatchSettingsHelper.GetOrCreateSettingsJson(SettingsId, SettingsFileName,
                () => new NetworkResiliencePatchSettings()
            );

            _instance.Save();
            return _instance;
        }

        public ProxyMode ProxyMode { get; set; } = ProxyMode.SystemProxy;
        public string CustomProxyAddress { get; set; } = string.Empty;

        public bool TryGetProxyUri(out Uri proxyUri)
        {
            if (!Uri.TryCreate(CustomProxyAddress, UriKind.Absolute, out proxyUri))
                return false;

            return proxyUri.Scheme == "http";
        }

        public void Save()
        {
            PatchSettingsHelper.SaveSettingsJson(SettingsId, SettingsFileName, this);
        }
    }

    internal enum ProxyMode
    {
        NoProxy,
        SystemProxy,
        CustomProxy
    }
}