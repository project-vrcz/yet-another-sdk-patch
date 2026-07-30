using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VRC.Core;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

// Adds the X-MacAddress header on each outgoing HttpRequestMessage so API.DeviceID is evaluated
// lazily. VRC.Core.API.DeviceID can throw (notably NullReferenceException) when accessed too early
// during editor/SDK initialization; computing the value at send time lets us skip the header when
// it is temporarily unavailable without failing factory construction. This also avoids coupling
// DeviceID evaluation to patch initialization order.
internal sealed class MacAddressHeaderHandler : DelegatingHandler
{
    private const string MacAddressHeaderName = "X-MacAddress";

    // Cache the first successfully retrieved DeviceID so it remains stable for this handler
    // (important when RandomizeDeviceIdPatch makes API.DeviceID return a new GUID each time).
    private readonly object _deviceIdLock = new();
    private string? _cachedDeviceId;

    private static readonly YesLogger Logger = new(nameof(MacAddressHeaderHandler));

    public MacAddressHeaderHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Remove(MacAddressHeaderName);

        if (TryGetDeviceId(out var deviceId))
        {
            request.Headers.TryAddWithoutValidation(MacAddressHeaderName, deviceId);
        }

        return base.SendAsync(request, cancellationToken);
    }

    private bool TryGetDeviceId(out string deviceId)
    {
        lock (_deviceIdLock)
        {
            if (_cachedDeviceId is not null)
            {
                deviceId = _cachedDeviceId;
                return true;
            }

            try
            {
                var currentDeviceId = API.DeviceID;
                if (string.IsNullOrEmpty(currentDeviceId))
                {
                    deviceId = string.Empty;
                    return false;
                }

                _cachedDeviceId = currentDeviceId;
                deviceId = currentDeviceId;
                return true;
            }
            catch (Exception ex)
            {
                // VRC.Core.API.DeviceID can throw a NullReferenceException when accessed too early
                // (e.g. before VRChat's internal API state has finished initializing). Don't substitute
                // a made-up value here, as that would be incorrect; just skip the header for this
                // request and try again the next time a request is sent.
                Logger.LogWarning(ex, "Failed to get VRC.Core.API.DeviceID, skipping the X-MacAddress header for this request.");
                deviceId = string.Empty;
                return false;
            }
        }
    }
}
