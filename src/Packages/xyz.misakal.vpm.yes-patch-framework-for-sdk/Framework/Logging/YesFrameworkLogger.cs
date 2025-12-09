using System;
using System.Collections.Generic;
using System.Linq;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;
using Object = UnityEngine.Object;

namespace YesPatchFrameworkForVRChatSdk.Logging;

internal sealed class YesFrameworkLogger : IYesLogger
{
    public static YesFrameworkLogger Instance { get; } = new();

    private readonly YesUnityDebugLogger _unityLogger = new();

    private readonly List<YesLogEntity> _logEntities = new();

    private readonly object _lock = new();

    public void Log(YesLogLevel level, string source, string message, Exception? exception, Object? context)
    {
        var logEntity = new YesLogEntity(level, source, message, exception, context);

        lock (_lock)
        {
            _logEntities.Add(logEntity);
        }

        _unityLogger.Log(level, source, message, exception, context);
    }

    public List<YesLogEntity> GetLogEntities()
    {
        lock (_lock)
        {
            return new List<YesLogEntity>(_logEntities);
        }
    }
}