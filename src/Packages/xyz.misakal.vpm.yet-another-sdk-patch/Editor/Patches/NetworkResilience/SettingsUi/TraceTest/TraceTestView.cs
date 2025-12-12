using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;
using YetAnotherPatchForVRChatSdk.Patches.NetworkResilience.RequestTest;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience.SettingsUi.TraceTest;

internal sealed class TraceTestView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<TraceTestView>
    {
    }

    private readonly Button _sendTestRequestButton;

    private readonly List<Label> _apiCloudUrlLabels;
    private readonly List<Label> _websiteUrlLabels;

    private readonly TextField _bestHttpApiCloudResultField;
    private readonly TextField _bestHttpWebsiteResultField;

    private readonly TextField _httpClientApiCloudResultField;
    private readonly TextField _httpClientWebsiteResultField;

    private const string VisualTreeAssetGuid = "87e6986966b7416a874cc5960d309e13";

    public TraceTestView()
    {
        var path = AssetDatabase.GUIDToAssetPath(VisualTreeAssetGuid);
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

        visualTree.CloneTree(this);

        _sendTestRequestButton = this.Q<Button>("send-test-request-button");

        _apiCloudUrlLabels = this.Query<Label>()
            .Class("api-cloud-url-label")
            .ToList();
        _websiteUrlLabels = this.Query<Label>()
            .Class("website-url-label")
            .ToList();

        _bestHttpApiCloudResultField = this.Q<TextField>("best-http-api-cloud-result");
        _bestHttpWebsiteResultField = this.Q<TextField>("best-http-website-result");

        _httpClientApiCloudResultField = this.Q<TextField>("httpclient-api-cloud-result");
        _httpClientWebsiteResultField = this.Q<TextField>("httpclient-website-result");

        foreach (var cloudUrlLabel in _apiCloudUrlLabels)
        {
            cloudUrlLabel.text = TraceRequestTest.VRChatApiEndpoint;
        }

        foreach (var websiteUrlLabel in _websiteUrlLabels)
        {
            websiteUrlLabel.text = TraceRequestTest.VRChatWebsiteEndpoint;
        }

        _sendTestRequestButton.clicked += RunTest;
    }

    private bool _isTestRunning;

    private async void RunTest()
    {
        if (_isTestRunning)
            return;

        _isTestRunning = true;
        await Task.WhenAll(
            RunHttpClientTest(TraceRequestTest.VRChatApiEndpoint, _httpClientApiCloudResultField),
            RunHttpClientTest(TraceRequestTest.VRChatWebsiteEndpoint, _httpClientWebsiteResultField),
            RunBestHttpTest(TraceRequestTest.VRChatApiEndpoint, _bestHttpApiCloudResultField),
            RunBestHttpTest(TraceRequestTest.VRChatWebsiteEndpoint, _bestHttpWebsiteResultField)
        );
        _isTestRunning = false;
    }

    private static async Task RunHttpClientTest(string endpoint, TextField field)
    {
        MainThreadDispatcher.Dispatch(() => field.value = "Running...");
        try
        {
            var result = await TraceRequestTest.VrcHttpClientSendTraceRequestAsync(endpoint);
            MainThreadDispatcher.Dispatch(() => field.value = result);
        }
        catch (Exception ex)
        {
            MainThreadDispatcher.Dispatch(() => field.value = ex.ToString());
        }
    }

    private static async Task RunBestHttpTest(string endpoint, TextField field)
    {
        MainThreadDispatcher.Dispatch(() => field.value = "Running...");
        try
        {
            var result = await TraceRequestTest.BestHttpSendTraceRequestAsync(endpoint);
            MainThreadDispatcher.Dispatch(() => field.value = result);
        }
        catch (Exception ex)
        {
            MainThreadDispatcher.Dispatch(() => field.value = ex.ToString());
        }
    }
}