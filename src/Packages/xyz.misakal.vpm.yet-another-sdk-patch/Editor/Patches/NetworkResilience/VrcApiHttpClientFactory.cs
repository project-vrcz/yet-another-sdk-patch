using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
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

    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public VrcApiHttpClientFactory(SetupCookieContainerGetCookiesDelegate? setupCookieContainer = null)
    {
        _setupCookieContainer = setupCookieContainer ?? (_ => { });

        _cookieContainer = new CookieContainer();
        _client = CreateClientInternal(_cookieContainer);
    }

    public HttpClient GetOrCreateClient()
    {
        _cookieContainer.Clear();
        _setupCookieContainer(_cookieContainer);

        return _client;
    }

    private HttpClient CreateClientInternal(CookieContainer cookieContainer)
    {
        _setupCookieContainer(cookieContainer);

        var innerHandler = new StandardSocketsHttpHandler
        {
            CookieContainer = cookieContainer,
            Proxy = new NetworkResilienceWebProxy(),
            ConnectTimeout = TimeSpan.FromSeconds(5),
            PooledConnectionIdleTimeout = TimeSpan.Zero
        };

        var handler = new ResilienceHttpHandler(new HttpLoggingHandler(innerHandler));
        var client = new HttpClient(handler);

        foreach (var header in _defaultRequestHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client;
    }
}