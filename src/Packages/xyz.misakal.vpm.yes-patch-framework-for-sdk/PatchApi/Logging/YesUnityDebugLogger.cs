using System;
using System.Text;
using Object = UnityEngine.Object;

namespace YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

internal sealed class YesUnityDebugLogger : IYesLogger
{
    public void Log(YesLogLevel level, string source, string message, Exception? exception, Object? context)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat("[{0}] [{1}] {2}", GetLogLevelString(level), GetColoredText(source, "#808080ff"), message);
        if (exception != null)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append(exception);
        }

        var logMessage = stringBuilder.ToString();

        switch (level)
        {
            case YesLogLevel.Trace:
            case YesLogLevel.Debug:
            case YesLogLevel.Info:
                UnityEngine.Debug.Log(logMessage, context);
                break;
            case YesLogLevel.Warning:
                UnityEngine.Debug.LogWarning(logMessage, context);
                break;
            case YesLogLevel.Error:
            case YesLogLevel.Critical:
                UnityEngine.Debug.LogError(logMessage, context);
                break;
            default:
                UnityEngine.Debug.Log(logMessage, context);
                break;
        }
    }

    private static string GetLogLevelString(YesLogLevel level)
    {
        var color = level switch
        {
            YesLogLevel.Trace => "808080", // Gray
            YesLogLevel.Debug => "00FFFF", // Cyan
            YesLogLevel.Info => "00FF00", // Green
            YesLogLevel.Warning => "FFFF00", // Yellow
            YesLogLevel.Error => "FF0000", // Red
            YesLogLevel.Critical => "FF00FF", // Magenta
            _ => "FFFFFF" // White
        };


        return GetColoredText(level.ToString(), color);
    }

    private static string GetColoredText(string text, string colorHex)
    {
        return $"<color=#{colorHex}>{text}</color>";
    }
}