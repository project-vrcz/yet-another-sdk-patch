using System;
using JetBrains.Annotations;

namespace YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

[PublicAPI]
public sealed partial class YesLogger
{
    private static IYesLogger _logger = new YesUnityDebugLogger();

    internal static void SetLogger(IYesLogger logger)
    {
        _logger = logger;
    }

    public static void Log(
        YesLogLevel level,
        Exception? exception,
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(level, source, message, exception, context);
    }

    public static void Log(
        YesLogLevel level,
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(level, source, message, null, context);
    }

    public static void Log(
        YesLogLevel level,
        string source,
        string message)
    {
        _logger.Log(level, source, message, null, null);
    }

    public static void LogTrace(
        Exception? exception,
        string source,
        string message)
    {
        _logger.Log(YesLogLevel.Trace, source, message, exception, null);
    }

    public static void LogTrace(
        Exception? exception,
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Trace, source, message, exception, context);
    }

    public static void LogTrace(
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Trace, source, message, null, context);
    }

    public static void LogTrace(
        string source,
        string message)
    {
        _logger.Log(YesLogLevel.Trace, source, message, null, null);
    }

    public static void LogDebug(
        Exception? exception,
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Debug, source, message, exception, context);
    }

    public static void LogDebug(
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Debug, source, message, null, context);
    }

    public static void LogDebug(
        string source,
        string message)
    {
        _logger.Log(YesLogLevel.Debug, source, message, null, null);
    }

    public static void LogInfo(
        Exception? exception,
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Info, source, message, exception, context);
    }

    public static void LogInfo(
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Info, source, message, null, context);
    }

    public static void LogInfo(
        string source,
        string message)
    {
        _logger.Log(YesLogLevel.Info, source, message, null, null);
    }

    public static void LogWarning(
        Exception? exception,
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Warning, source, message, exception, context);
    }

    public static void LogWarning(
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Warning, source, message, null, context);
    }

    public static void LogWarning(
        string source,
        string message)
    {
        _logger.Log(YesLogLevel.Warning, source, message, null, null);
    }

    public static void LogError(
        Exception? exception,
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Error, source, message, exception, context);
    }

    public static void LogError(
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Error, source, message, null, context);
    }

    public static void LogError(
        string source,
        string message)
    {
        _logger.Log(YesLogLevel.Error, source, message, null, null);
    }

    public static void LogCritical(
        Exception? exception,
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Critical, source, message, exception, context);
    }

    public static void LogCritical(
        string source,
        string message,
        UnityEngine.Object? context)
    {
        _logger.Log(YesLogLevel.Critical, source, message, null, context);
    }

    public static void LogCritical(
        string source,
        string message)
    {
        _logger.Log(YesLogLevel.Critical, source, message, null, null);
    }
}