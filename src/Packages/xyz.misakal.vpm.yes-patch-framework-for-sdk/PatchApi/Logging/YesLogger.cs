using System;

namespace YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

public sealed partial class YesLogger
{
    public string Source { get; }

    public YesLogger(string source)
    {
        Source = source;
    }

    public void Log(
        YesLogLevel level,
        Exception? exception,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(level, Source, message, exception, context);
    }

    public void Log(
        YesLogLevel level,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(level, Source, message, null, context);
    }

    public void Log(
        YesLogLevel level,
        string message)
    {
        _logger.Log(level, Source, message, null, null);
    }

    public void LogTrace(
        Exception? exception,
        string message)
    {
        _logger.Log(YesLogLevel.Trace, Source, message, exception, null);
    }

    public void LogTrace(
        Exception? exception,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Trace, Source, message, exception, context);
    }

    public void LogTrace(
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Trace, Source, message, null, context);
    }

    public void LogTrace(string message)
    {
        _logger.Log(YesLogLevel.Trace, Source, message, null, null);
    }

    public void LogDebug(
        Exception? exception,
        string message)
    {
        _logger.Log(YesLogLevel.Debug, Source, message, exception, null);
    }

    public void LogDebug(
        Exception? exception,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Debug, Source, message, exception, context);
    }

    public void LogDebug(
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Debug, Source, message, null, context);
    }

    public void LogDebug(string message)
    {
        _logger.Log(YesLogLevel.Debug, Source, message, null, null);
    }

    public void LogInfo(
        Exception? exception,
        string message)
    {
        _logger.Log(YesLogLevel.Info, Source, message, exception, null);
    }

    public void LogInfo(
        Exception? exception,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Info, Source, message, exception, context);
    }

    public void LogInfo(
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Info, Source, message, null, context);
    }

    public void LogInfo(string message)
    {
        _logger.Log(YesLogLevel.Info, Source, message, null, null);
    }

    public void LogError(
        Exception? exception,
        string message)
    {
        _logger.Log(YesLogLevel.Error, Source, message, exception, null);
    }

    public void LogError(
        Exception? exception,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Error, Source, message, exception, context);
    }

    public void LogError(
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Error, Source, message, null, context);
    }

    public void LogError(string message)
    {
        _logger.Log(YesLogLevel.Error, Source, message, null, null);
    }

    public void LogWarning(
        Exception? exception,
        string message)
    {
        _logger.Log(YesLogLevel.Warning, Source, message, exception, null);
    }

    public void LogWarning(
        Exception? exception,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Warning, Source, message, exception, context);
    }

    public void LogWarning(
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Warning, Source, message, null, context);
    }

    public void LogWarning(string message)
    {
        _logger.Log(YesLogLevel.Warning, Source, message, null, null);
    }

    public void LogCritical(
        Exception? exception,
        string message)
    {
        _logger.Log(YesLogLevel.Critical, Source, message, exception, null);
    }

    public void LogCritical(
        Exception? exception,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Critical, Source, message, exception, context);
    }

    public void LogCritical(
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Critical, Source, message, null, context);
    }

    public void LogCritical(string message)
    {
        _logger.Log(YesLogLevel.Critical, Source, message, null, null);
    }
}