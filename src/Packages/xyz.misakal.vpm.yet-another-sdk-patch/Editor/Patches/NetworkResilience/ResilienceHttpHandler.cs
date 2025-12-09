using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using UnityEngine;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

internal sealed class ResilienceHttpHandler : DelegatingHandler
{
    private const string LoggerSource = nameof(NetworkResiliencePatch) + "." + nameof(ResilienceHttpHandler);

    private readonly ResiliencePipeline _pipeline;

    public ResilienceHttpHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
        var options = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<HttpRequestException>()
                .HandleInner<IOException>()
                .HandleInner<SocketException>()
                .HandleInner<TimeoutException>(),
            MaxRetryAttempts = 5,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromSeconds(3),
            OnRetry = arguments =>
            {
                YesLogger.LogWarning(
                    arguments.Outcome.Exception,
                    LoggerSource,
                    $"Retrying HTTP request. Attempt {arguments.AttemptNumber}. Delay {arguments.RetryDelay.ToString()}.",
                    null);

                return default;
            }
        };

        var builder = new ResiliencePipelineBuilder()
            .AddRetry(options);

        _pipeline = builder.Build();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _pipeline.ExecuteAsync(
                    async token => await base.SendAsync(request, token),
                    cancellationToken)
                .AsTask();
        }
        catch (Exception ex)
        {
            YesLogger.LogError(ex, LoggerSource, "HTTP request failed after retries.", null);
            throw;
        }
    }
}