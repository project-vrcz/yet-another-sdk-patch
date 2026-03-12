using System;
using System.Net;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

internal sealed class NetworkResilienceWebProxy : IWebProxy
{
    private readonly NetworkResiliencePatchSettings _settings = NetworkResiliencePatchSettings.GetOrCreateSettings();
    private CustomWebProxyInstance? _customWebProxyInstance;

    public Uri GetProxy(Uri destination)
    {
        return GetWebProxy()?.GetProxy(destination) ?? destination;
    }

    public bool IsBypassed(Uri host)
    {
        return GetWebProxy()?.IsBypassed(host) ?? true;
    }

    public ICredentials Credentials { get; set; } = CredentialCache.DefaultCredentials;

    private IWebProxy? GetWebProxy()
    {
        switch (_settings.ProxyMode)
        {
            case ProxyMode.NoProxy:
                return null;
            case ProxyMode.CustomProxy:
                return GetCustomWebProxy();
            case ProxyMode.SystemProxy:
            default:
                return WebRequest.GetSystemWebProxy();
        }
    }

    private IWebProxy GetCustomWebProxy()
    {
        var customProxyUri = _settings.CustomProxyAddress;
        if (_customWebProxyInstance?.CustomProxyUri == customProxyUri)
            return _customWebProxyInstance.Value.CustomProxy;

        if (!_settings.TryGetProxyUri(out var proxyUri))
            return WebRequest.GetSystemWebProxy();

        var webProxy = new WebProxy(proxyUri);
        _customWebProxyInstance = new CustomWebProxyInstance
        {
            CustomProxy = webProxy,
            CustomProxyUri = customProxyUri
        };

        return webProxy;
    }

    private struct CustomWebProxyInstance
    {
        public IWebProxy CustomProxy;
        public string CustomProxyUri;
    }
}