using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.Extensions;
using YesPatchFrameworkForVRChatSdk.Logging;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;
using YesPatchFrameworkForVRChatSdk.UserInterface.Controls.Logging;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Views.Logging;

internal sealed class YesLoggingView : VisualElement
{
    private readonly List<YesLogEntity> _logEntries = YesFrameworkLogger.Instance.GetLogEntities();
    private readonly List<YesLogEntity> _filteredLogEntries = new();

    private readonly ListView _logListView;

    private readonly Button _pingSelectedLogEntityContextButton;
    private readonly TextField _selectedLogDetailsTextField;

    private readonly TextField _searchKeywordField;
    private readonly ToolbarToggle _traceLevelFilterToggle;
    private readonly ToolbarToggle _debugLevelFilterToggle;
    private readonly ToolbarToggle _infoLevelFilterToggle;
    private readonly ToolbarToggle _warningLevelFilterToggle;
    private readonly ToolbarToggle _errorLevelFilterToggle;
    private readonly ToolbarToggle _criticalLevelFilterToggle;

    private readonly ToolbarToggle _autoScrollToggle;

    private YesLogEntity? _selectedLogEntity;

    private const string VisualTreeAssetGuid = "ad3f8c8e614d488aa906eec99e746a04";

    public YesLoggingView()
    {
        var tree = AssetDatabaseExtenstion.LoadAssetFromGuid<VisualTreeAsset>(VisualTreeAssetGuid);
        if (tree == null)
            throw new FileNotFoundException(
                $"Failed to load YesLoggingView UXML asset: YesLoggingView.uxml ({VisualTreeAssetGuid})");
        tree.CloneTree(this);

        _logListView = this.Q<ListView>("log-list-view");
        _selectedLogDetailsTextField = this.Q<TextField>("selected-log-message-field");
        _pingSelectedLogEntityContextButton = this.Q<Button>("ping-context-button");
        _pingSelectedLogEntityContextButton.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

        _pingSelectedLogEntityContextButton.clicked += () =>
        {
            if (_selectedLogEntity is null || !_selectedLogEntity.Context)
                return;

            EditorGUIUtility.PingObject(_selectedLogEntity.Context);
        };

        _logListView.itemsSource = _filteredLogEntries;
        _logListView.makeItem = () => new VisualElement();
        _logListView.bindItem = (element, i) =>
        {
            element.Clear();
            var logEntity = (YesLogEntity)_logListView.itemsSource[i];
            var logItem = new YesLoggingListItem(logEntity);

            element.Add(logItem);
        };

        _logListView.selectionChanged += selectedItems =>
        {
            if (selectedItems.FirstOrDefault() is not YesLogEntity logEntity)
                return;

            _selectedLogEntity = logEntity;
            _selectedLogDetailsTextField.value = GetLogEntityDetails(logEntity);

            if (logEntity.Context != null)
            {
                EditorGUIUtility.PingObject(logEntity.Context);
                _pingSelectedLogEntityContextButton.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else
            {
                _pingSelectedLogEntityContextButton.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
        };

        _logListView.fixedItemHeight = 50;

        _searchKeywordField = this.Q<TextField>("logging-filter-keyword-field");
        _searchKeywordField.RegisterValueChangedCallback(_ => UpdateFilteredLogEntries());

        _traceLevelFilterToggle = this.Q<ToolbarToggle>("trace-filter-toggle");
        _debugLevelFilterToggle = this.Q<ToolbarToggle>("debug-filter-toggle");
        _infoLevelFilterToggle = this.Q<ToolbarToggle>("info-filter-toggle");
        _warningLevelFilterToggle = this.Q<ToolbarToggle>("warning-filter-toggle");
        _errorLevelFilterToggle = this.Q<ToolbarToggle>("error-filter-toggle");
        _criticalLevelFilterToggle = this.Q<ToolbarToggle>("critical-filter-toggle");
        _traceLevelFilterToggle.RegisterValueChangedCallback(_ => UpdateFilteredLogEntries());
        _debugLevelFilterToggle.RegisterValueChangedCallback(_ => UpdateFilteredLogEntries());
        _infoLevelFilterToggle.RegisterValueChangedCallback(_ => UpdateFilteredLogEntries());
        _warningLevelFilterToggle.RegisterValueChangedCallback(_ => UpdateFilteredLogEntries());
        _errorLevelFilterToggle.RegisterValueChangedCallback(_ => UpdateFilteredLogEntries());
        _criticalLevelFilterToggle.RegisterValueChangedCallback(_ => UpdateFilteredLogEntries());

        _autoScrollToggle = this.Q<ToolbarToggle>("auto-scroll-toggle");
        _autoScrollToggle.RegisterValueChangedCallback(_ =>
        {
            if (_autoScrollToggle.value)
                ScrollToBottom();
        });

        RegisterCallback<AttachToPanelEvent>(_ =>
        {
            YesFrameworkLogger.Instance.OnLogEntityAdded += OnInstanceOnOnLogEntityAdded;
        });

        RegisterCallback<DetachFromPanelEvent>(_ =>
        {
            YesFrameworkLogger.Instance.OnLogEntityAdded -= OnInstanceOnOnLogEntityAdded;
        });

        UpdateFilteredLogEntries();
    }

    private void UpdateFilteredLogEntries()
    {
        _filteredLogEntries.Clear();

        var result = _logEntries
            .Where(log => (log.Level == YesLogLevel.Trace && _traceLevelFilterToggle.value) ||
                          (log.Level == YesLogLevel.Debug && _debugLevelFilterToggle.value) ||
                          (log.Level == YesLogLevel.Info && _infoLevelFilterToggle.value) ||
                          (log.Level == YesLogLevel.Warning && _warningLevelFilterToggle.value) ||
                          (log.Level == YesLogLevel.Error && _errorLevelFilterToggle.value) ||
                          (log.Level == YesLogLevel.Critical && _criticalLevelFilterToggle.value))
            .Where(log =>
                log.FullMessage.Contains(_searchKeywordField.value, StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

        _filteredLogEntries.AddRange(result);
        _logListView.RefreshItems();

        if (_autoScrollToggle.value)
            ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        if (_filteredLogEntries.Count == 0)
            return;

        _logListView.schedule.Execute(_ =>
            _logListView.ScrollToItem(_filteredLogEntries.Count - 1)
        );
    }

    private void OnInstanceOnOnLogEntityAdded(object _, YesLogEntity entity)
    {
        _logEntries.Add(entity);

        MainThreadDispatcher.Dispatch(UpdateFilteredLogEntries);
    }

    private static string GetLogEntityDetails(YesLogEntity logEntity)
    {
        var details = $"Source: {logEntity.Source}\n" +
                      $"Level: {logEntity.Level}\n" +
                      $"Message: {logEntity.Message}\n";

        if (logEntity.Exception is { } ex)
        {
            details += $"\nException:\n{ex}";
        }

        if (logEntity.Context != null)
        {
            details += $"\n\nContext: {logEntity.Context} (InstanceID: {logEntity.Context.GetInstanceID()})\n";
        }

        return details;
    }
}