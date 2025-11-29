using YesPatchFrameworkForVRChatSdk.PatchApi;
using YetAnotherPatchForVRChatSdk.Patches;

[assembly: ExportYesPatch(typeof(TestPatch))]
[assembly: ExportYesPatch(typeof(RemoteConfigCachePatch))]