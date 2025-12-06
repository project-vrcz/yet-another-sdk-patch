using YesPatchFrameworkForVRChatSdk.PatchApi;
using YetAnotherPatchForVRChatSdk.Patches;
using YetAnotherPatchForVRChatSdk.Patches.NetworkResilience;

[assembly: ExportYesPatch(typeof(RandomizeDeviceIdPatch))]
[assembly: ExportYesPatch(typeof(NetworkResiliencePatch))]
[assembly: ExportYesPatch(typeof(RemoteConfigCachePatch))]
[assembly: ExportYesPatch(typeof(AlwaysAgreeCopyrightAgreementPatch))]