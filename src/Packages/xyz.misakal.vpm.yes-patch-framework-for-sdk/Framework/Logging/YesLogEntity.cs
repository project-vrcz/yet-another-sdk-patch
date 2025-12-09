using System;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;
using Object = UnityEngine.Object;

namespace YesPatchFrameworkForVRChatSdk.Logging;

internal class YesLogEntity
{
    public YesLogEntity(YesLogLevel level, string source, string message, Exception? exception, Object? context)
    {
        Level = level;
        Source = source;
        Message = message;
        Exception = exception;
        Context = context;
    }

    public YesLogLevel Level { get; }
    public string Source { get; }
    public string Message { get; }

    public Exception? Exception { get; }
    public UnityEngine.Object? Context { get; }
}