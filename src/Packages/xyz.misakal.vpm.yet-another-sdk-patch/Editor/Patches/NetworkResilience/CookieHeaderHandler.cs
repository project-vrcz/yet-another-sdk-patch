using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YetAnotherPatchForVRChatSdk.Extensions;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

internal sealed class CookieHeaderHandler : DelegatingHandler
{
    private const string CookieHeaderName = "Cookie";
    private const string SetCookieHeaderName = "Set-Cookie";

    private CookieContainerState _state;

    public CookieHeaderHandler(CookieContainer cookieContainer, HttpMessageHandler innerHandler) : base(innerHandler)
    {
        _state = new CookieContainerState(cookieContainer);
    }

    public void SetCookieContainer(CookieContainer cookieContainer)
    {
        Volatile.Write(ref _state, new CookieContainerState(cookieContainer));
    }

    public CookieContainer CreateCookieContainerSnapshot()
    {
        var state = Volatile.Read(ref _state);
        lock (state.SyncRoot)
        {
            return state.CookieContainer.Clone();
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestUri = request.RequestUri;
        var state = Volatile.Read(ref _state);
        lock (state.SyncRoot)
        {
            ApplyCookieHeader(state.CookieContainer, request);
        }

        var response = await base.SendAsync(request, cancellationToken);

        var responseRequestUri = response.RequestMessage?.RequestUri ?? requestUri;
        if (responseRequestUri is { IsAbsoluteUri: true } absoluteResponseRequestUri &&
            response.Headers.TryGetValues(SetCookieHeaderName, out var setCookieHeaders))
        {
            lock (state.SyncRoot)
            {
                foreach (var setCookieHeader in setCookieHeaders)
                {
                    state.CookieContainer.SetCookies(absoluteResponseRequestUri, setCookieHeader);
                }
            }
        }

        return response;
    }

    private static void ApplyCookieHeader(CookieContainer cookieContainer, HttpRequestMessage request)
    {
        var existingCookieHeader = request.Headers.TryGetValues(CookieHeaderName, out var existingCookieValues)
            ? string.Join("; ", existingCookieValues)
            : null;

        request.Headers.Remove(CookieHeaderName);

        string? cookieHeader = null;
        if (request.RequestUri is { IsAbsoluteUri: true } requestUri)
            cookieHeader = cookieContainer.GetCookieHeader(requestUri);

        if (!string.IsNullOrEmpty(existingCookieHeader))
        {
            cookieHeader = string.IsNullOrEmpty(cookieHeader)
                ? existingCookieHeader
                : $"{existingCookieHeader}; {cookieHeader}";
        }

        if (!string.IsNullOrEmpty(cookieHeader))
            request.Headers.TryAddWithoutValidation(CookieHeaderName, cookieHeader);
    }

    private sealed class CookieContainerState
    {
        public CookieContainerState(CookieContainer cookieContainer)
        {
            CookieContainer = cookieContainer;
        }

        public readonly object SyncRoot = new();
        public readonly CookieContainer CookieContainer;
    }
}
