using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using UnityEngine;
using VRC;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

internal sealed class VrcApiHttpClientFactory
{
    public delegate void SetupCookieContainerGetCookiesDelegate(CookieContainer cookieContainer);

    private readonly SetupCookieContainerGetCookiesDelegate _setupCookieContainer;

    private readonly Dictionary<string, string> _defaultRequestHeaders = new()
    {
        { "User-Agent", "VRC.Core.BestHTTP" },
        { "X-SDK-Version", Tools.SdkVersion },
        { "X-Platform", Tools.Platform },
        { "X-Unity-Version", Application.unityVersion },
        { "Accept", "application/json" }
    };

    private readonly object _lock = new();
    private readonly HashSet<string> _managedCookieKeys = new();
    private readonly CookieHeaderHandler _cookieHeaderHandler;
    private readonly HttpClient _client;

    public VrcApiHttpClientFactory(SetupCookieContainerGetCookiesDelegate? setupCookieContainer = null)
    {
        _setupCookieContainer = setupCookieContainer ?? (_ => { });

        var cookieContainer = CreateCookieContainer();
        var innerHandler = new StandardSocketsHttpHandler
        {
            // CookieHeaderHandler owns the CookieContainer state and applies request/response
            // cookies under a per-generation lock. The underlying handler must not mutate it too.
            UseCookies = false,
            Proxy = new NetworkResilienceWebProxy(),
            ConnectTimeout = TimeSpan.FromSeconds(5),
            PooledConnectionIdleTimeout = TimeSpan.Zero
        };

        _cookieHeaderHandler = new CookieHeaderHandler(cookieContainer, innerHandler);
        _client = CreateClientInternal(_cookieHeaderHandler);
    }

    // Exposes the underlying HttpClient instance without mutating any shared state, so callers
    // that only need to compare the client reference (e.g. to identify which client an HttpClient
    // instance belongs to) don't trigger cookie/header refreshes as a side effect.
    public HttpClient Client => _client;

    public HttpClient GetOrCreateClient()
    {
        lock (_lock)
        {
            // Copy cookies received by the current generation before applying the latest SDK
            // credentials. Publish the new generation only after it is fully configured; requests
            // already in flight keep using and updating the old generation safely.
            var currentCookieContainer = _cookieHeaderHandler.CreateCookieContainerSnapshot();
            var cookieContainer = CreateCookieContainer(currentCookieContainer);
            _cookieHeaderHandler.SetCookieContainer(cookieContainer);

            return _client;
        }
    }

    private CookieContainer CreateCookieContainer(CookieContainer? currentCookieContainer = null)
    {
        // Preserve cookies received from the server, but rebuild the cookies owned by the setup
        // callback so credentials that disappeared from the SDK are not retained by the snapshot.
        var cookieContainer = currentCookieContainer?.Clone(cookie =>
            !_managedCookieKeys.Contains(GetCookieKey(cookie))) ?? new CookieContainer();

        var managedCookieContainer = new CookieContainer();
        _setupCookieContainer(managedCookieContainer);

        var managedCookieKeys = new HashSet<string>();
        foreach (var cookie in managedCookieContainer.GetAllCookies())
        {
            managedCookieKeys.Add(GetCookieKey(cookie));
        }

        cookieContainer.AddRange(managedCookieContainer.GetAllCookies());
        _managedCookieKeys.Clear();
        _managedCookieKeys.UnionWith(managedCookieKeys);

        return cookieContainer;
    }

    private static string GetCookieKey(Cookie cookie)
    {
        return string.Join("\u001f", cookie.Name, cookie.Domain, cookie.Path);
    }

    private HttpClient CreateClientInternal(HttpMessageHandler innerHandler)
    {
        var handler = new ResilienceHttpHandler(new MacAddressHeaderHandler(new HttpLoggingHandler(innerHandler)));
        var client = new HttpClient(handler);
        client.Timeout = Timeout.InfiniteTimeSpan;

        foreach (var header in _defaultRequestHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client;
    }
}