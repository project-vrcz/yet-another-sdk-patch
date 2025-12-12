using System;
using System.Net.Http;
using System.Threading.Tasks;
using BestHTTP;
using HarmonyLib;
using VRC.SDKBase.Editor.Api;
using YetAnotherPatchForVRChatSdk.Patches.NetworkResilience.Exceptions;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience.RequestTest;

internal static class TraceRequestTest
{
    public const string VRChatApiEndpoint = "https://api.vrchat.cloud";
    public const string VRChatWebsiteEndpoint = "https://vrchat.com";

    public const string CloudflareTracePath = "/cdn-cgi/trace";

    public static async ValueTask<string> VrcHttpClientSendTraceRequestAsync(string endpoint)
    {
        var requestUri = new Uri(new Uri(endpoint), CloudflareTracePath);
        var client = GetVrcApiHttpClient(requestUri);
        return await client.GetStringAsync(requestUri);
    }

    public static async ValueTask<string> BestHttpSendTraceRequestAsync(string endpoint)
    {
        var tcs = new TaskCompletionSource<string>();
        var request = new HTTPRequest(
            new Uri(new Uri(endpoint),
                CloudflareTracePath),
            HTTPMethods.Get,
            false,
            true,
            (request, response) =>
            {
                switch (request.State)
                {
                    case HTTPRequestStates.Finished:
                        tcs.SetResult(response.DataAsText);
                        return;
                    case HTTPRequestStates.Error:
                        tcs.SetException(new BestHttpRequestException(request.Exception));
                        break;
                    case HTTPRequestStates.Aborted:
                        tcs.SetException(new BestHttpRequestAbortedException());
                        break;
                    case HTTPRequestStates.ConnectionTimedOut:
                        tcs.SetException(new BestHttpRequestConnectionTimeoutException());
                        break;
                    case HTTPRequestStates.TimedOut:
                        tcs.SetException(new BestHttpRequestTimeoutException());
                        break;
                    default:
                        tcs.SetException(new BestHttpRequestUnexpectedStateException(request.State));
                        break;
                }
            });

        request.Send();

        return await tcs.Task;
    }

    public static HttpClient GetVrcApiHttpClient(Uri url)
    {
        var getClientMethod =
            AccessTools.Method(typeof(VRCApi), "GetClient", new[] { typeof(Uri) });
        if (getClientMethod is null || !getClientMethod.IsStatic || getClientMethod.ReturnType != typeof(HttpClient))
            throw new Exception("Could not find static VRCApi.GetClient(Uri url) method.");

        var result = getClientMethod.Invoke(null, new object[] { url });
        if (result is not HttpClient client)
            throw new Exception("VRCApi.GetClient did not return an HttpClient.");

        return client;
    }
}