using UnityEditor;
using YesPatchFrameworkForVRChatSdk.Logging;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;
using YesPatchFrameworkForVRChatSdk.PatchManagement;

namespace YesPatchFrameworkForVRChatSdk
{
    internal static class EntryPoint
    {
        [InitializeOnLoadMethod]
        public static void Main()
        {
            YesLogger.SetLogger(YesFrameworkLogger.Instance);
            YesPatchManager.Instance.ApplyPatches();

            YesFrameworkLogger.SetLogLevelMenuChecks();
        }
    }
}