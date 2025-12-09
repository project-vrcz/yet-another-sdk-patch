using System;

namespace YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

internal interface IYesLogger
{
    void Log(YesLogLevel level, string source, string message, Exception? exception, UnityEngine.Object? context);
}