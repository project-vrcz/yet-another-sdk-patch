using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

internal sealed class HttpLoggingHandler : DelegatingHandler
{
    private const string LoggerSource = nameof(NetworkResiliencePatch) + "." + nameof(HttpLoggingHandler);

    public HttpLoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        YesLogger.LogDebug(LoggerSource, $"{request.Method} {request.RequestUri.GetLeftPart(UriPartial.Path)}");
        return base.SendAsync(request, cancellationToken);
    }
}