using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.Logging;
using YesPatchFrameworkForVRChatSdk.UserInterface.Controls.Logging;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Views.Logging;

internal sealed class YesLoggingView : VisualElement
{
    private readonly List<YesLogEntity> _logEntries = YesFrameworkLogger.Instance.GetLogEntities();

    private readonly ListView _logListView;

    private readonly Button _pingSelectedLogEntityContextButton;
    private readonly TextField _selectedLogDetailsTextField;

    private YesLogEntity? _selectedLogEntity;

    public YesLoggingView()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/xyz.misakal.vpm.yes-patch-framework-for-sdk/Framework/UserInterface/Views/Logging/" +
            nameof(YesLoggingView) + ".uxml");
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

        _logListView.itemsSource = _logEntries;
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

        return details;
    }
}