using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using UnityEngine;
using VRC;
using VRC.Core;
using YetAnotherPatchForVRChatSdk.Extensions;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

internal sealed class VrcApiHttpClientFactory
{
    public delegate void SetupCookieContainerGetCookiesDelegate(CookieContainer cookieContainer);

    private readonly SetupCookieContainerGetCookiesDelegate _setupCookieContainer;

    private readonly Dictionary<string, string> _defaultRequestHeaders = new()
    {
        { "User-Agent", "VRC.Core.BestHTTP" },
        { "X-MacAddress", API.DeviceID },
        { "X-SDK-Version", Tools.SdkVersion },
        { "X-Platform", Tools.Platform },
        { "X-Unity-Version", Application.unityVersion },
        { "Accept", "application/json" }
    };

    private VrcApiHttpClientInstance? _clientInstance;

    public VrcApiHttpClientFactory(SetupCookieContainerGetCookiesDelegate? setupCookieContainer = null)
    {
        _setupCookieContainer = setupCookieContainer ?? (_ => { });
    }

    public HttpClient GetOrCreateClient()
    {
        if (_clientInstance is not null)
        {
            var cookieContainer = _clientInstance.CookieContainer;
            cookieContainer.Clear();
            _setupCookieContainer(cookieContainer);

            return _clientInstance.Client;
        }

        return CreateClientCore();
    }

    private HttpClient CreateClientCore()
    {
        var cookieContainer = new CookieContainer();
        _setupCookieContainer(cookieContainer);

        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer
        };

        var client = new HttpClient(handler);
        foreach (var header in _defaultRequestHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        _clientInstance = new VrcApiHttpClientInstance(client, cookieContainer);
        return client;
    }

    private sealed class VrcApiHttpClientInstance
    {
        public readonly HttpClient Client;
        public readonly CookieContainer CookieContainer;

        public VrcApiHttpClientInstance(HttpClient client, CookieContainer cookieContainer)
        {
            CookieContainer = cookieContainer;
            Client = client;
        }
    }
}