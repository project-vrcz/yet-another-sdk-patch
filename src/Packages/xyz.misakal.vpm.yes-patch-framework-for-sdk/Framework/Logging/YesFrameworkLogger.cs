using System;
using System.Collections.Generic;
using UnityEditor;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;
using YesPatchFrameworkForVRChatSdk.Settings.PatchManager;
using Object = UnityEngine.Object;

namespace YesPatchFrameworkForVRChatSdk.Logging;

internal sealed class YesFrameworkLogger : IYesLogger
{
    public static YesFrameworkLogger Instance { get; } = new();

    private readonly YesUnityDebugLogger _unityLogger = new();
    private YesLogLevel _minLogLevelForUnity = YesLogLevel.Info;

    private readonly List<YesLogEntity> _logEntities = new();
    private readonly object _lock = new();

    public event EventHandler<YesLogEntity>? OnLogEntityAdded;

    public YesFrameworkLogger()
    {
        var settings = YesPatchManagerSettings.GetOrCreateSettings();
        _minLogLevelForUnity = settings.unityConsoleMinLogLevel;
    }

    public void Log(YesLogLevel level, string source, string message, Exception? exception, Object? context)
    {
        var logEntity = new YesLogEntity(level, source, message, exception, context);

        lock (_lock)
        {
            _logEntities.Add(logEntity);
            OnOnLogEntityAdded(logEntity);
        }

        if (level >= _minLogLevelForUnity)
            _unityLogger.Log(level, source, message, exception, context);
    }

    public List<YesLogEntity> GetLogEntities()
    {
        lock (_lock)
        {
            return new List<YesLogEntity>(_logEntities);
        }
    }

    private void OnOnLogEntityAdded(YesLogEntity e)
    {
        OnLogEntityAdded?.Invoke(this, e);
    }

    #region Unity Menu Item & Settings

    private const string MinimumLogLevelMenuRoot =
        FrameworkMenuItem.ToolsMenuRoot + "Minimum Log Level (Unity Console)/";

    [MenuItem(MinimumLogLevelMenuRoot + "Trace", false, 1)]
    private static void SetMinLogLevelTrace()
    {
        Instance._minLogLevelForUnity = YesLogLevel.Trace;
        SetLogLevelMenuChecks();
        SaveLogLevelToSettings();
    }

    [MenuItem(MinimumLogLevelMenuRoot + "Debug", false, 2)]
    private static void SetMinLogLevelDebug()
    {
        Instance._minLogLevelForUnity = YesLogLevel.Debug;
        SetLogLevelMenuChecks();
        SaveLogLevelToSettings();
    }

    [MenuItem(MinimumLogLevelMenuRoot + "Info", false, 3)]
    private static void SetMinLogLevelInfo()
    {
        Instance._minLogLevelForUnity = YesLogLevel.Info;
        SetLogLevelMenuChecks();
        SaveLogLevelToSettings();
    }

    [MenuItem(MinimumLogLevelMenuRoot + "Warning", false, 4)]
    private static void SetMinLogLevelWarning()
    {
        Instance._minLogLevelForUnity = YesLogLevel.Warning;
        SetLogLevelMenuChecks();
        SaveLogLevelToSettings();
    }

    [MenuItem(MinimumLogLevelMenuRoot + "Error", false, 5)]
    private static void SetMinLogLevelError()
    {
        Instance._minLogLevelForUnity = YesLogLevel.Error;
        SetLogLevelMenuChecks();
        SaveLogLevelToSettings();
    }

    [MenuItem(MinimumLogLevelMenuRoot + "Critical", false, 6)]
    private static void SetMinLogLevelCritical()
    {
        Instance._minLogLevelForUnity = YesLogLevel.Critical;
        SetLogLevelMenuChecks();
        SaveLogLevelToSettings();
    }

    private static void ResetMenuChecks()
    {
        Menu.SetChecked(MinimumLogLevelMenuRoot + "Trace", false);
        Menu.SetChecked(MinimumLogLevelMenuRoot + "Debug", false);
        Menu.SetChecked(MinimumLogLevelMenuRoot + "Info", false);
        Menu.SetChecked(MinimumLogLevelMenuRoot + "Warning", false);
        Menu.SetChecked(MinimumLogLevelMenuRoot + "Error", false);
        Menu.SetChecked(MinimumLogLevelMenuRoot + "Critical", false);
    }

    public static void SetLogLevelMenuChecks()
    {
        ResetMenuChecks();
        switch (Instance._minLogLevelForUnity)
        {
            case YesLogLevel.Trace:
                Menu.SetChecked(MinimumLogLevelMenuRoot + "Trace", true);
                break;
            case YesLogLevel.Debug:
                Menu.SetChecked(MinimumLogLevelMenuRoot + "Debug", true);
                break;
            case YesLogLevel.Info:
                Menu.SetChecked(MinimumLogLevelMenuRoot + "Info", true);
                break;
            case YesLogLevel.Warning:
                Menu.SetChecked(MinimumLogLevelMenuRoot + "Warning", true);
                break;
            case YesLogLevel.Error:
                Menu.SetChecked(MinimumLogLevelMenuRoot + "Error", true);
                break;
            case YesLogLevel.Critical:
                Menu.SetChecked(MinimumLogLevelMenuRoot + "Critical", true);
                break;
        }
    }

    private static void SaveLogLevelToSettings()
    {
        var settings = YesPatchManagerSettings.GetOrCreateSettings();
        settings.unityConsoleMinLogLevel = Instance._minLogLevelForUnity;
        settings.Save();
    }

    #endregion
}