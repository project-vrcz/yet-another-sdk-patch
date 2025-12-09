using UnityEditor;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.Logging;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Controls.Logging;

internal sealed class YesLoggingListItem : VisualElement
{
    private readonly YesLogEntity _logEntity;

    private readonly Label _sourceLabel;
    private readonly Label _levelLabel;
    private readonly Label _messageLabel;

    public YesLoggingListItem(YesLogEntity logEntity)
    {
        _logEntity = logEntity;

        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/xyz.misakal.vpm.yes-patch-framework-for-sdk/Framework/UserInterface/Controls/Logging/" +
            nameof(YesLoggingListItem) + ".uxml");
        tree.CloneTree(this);

        _sourceLabel = this.Q<Label>("log-source");
        _levelLabel = this.Q<Label>("log-level");
        _messageLabel = this.Q<Label>("log-message");

        _sourceLabel.text = _logEntity.Source;
        _levelLabel.text = _logEntity.Level.ToString();
        _levelLabel.AddToClassList(_logEntity.Level.ToString().ToLowerInvariant());

        _messageLabel.text = _logEntity.Message;
        if (_logEntity.Exception is not { } ex)
            return;

        _messageLabel.text += "\n" + ex.GetType().Name + ": " + ex.Message;
    }
}