using System.IO;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.Extensions;
using YesPatchFrameworkForVRChatSdk.Logging;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Controls.Logging;

internal sealed class YesLoggingListItem : VisualElement
{
    private readonly YesLogEntity _logEntity;

    private readonly Label _sourceLabel;
    private readonly Label _levelLabel;
    private readonly Label _messageLabel;

    private const string VisualTreeAssetGuid = "e6374f07026a44d49a7856742d04698e";

    public YesLoggingListItem(YesLogEntity logEntity)
    {
        _logEntity = logEntity;

        var tree = AssetDatabaseExtenstion.LoadAssetFromGuid<VisualTreeAsset>(VisualTreeAssetGuid);
        if (tree == null)
            throw new FileNotFoundException(
                $"Failed to load YesLoggingListItem UXML asset: YesLoggingListItem.uxml ({VisualTreeAssetGuid})");
        tree.CloneTree(this);

        _sourceLabel = this.Q<Label>("log-source");
        _levelLabel = this.Q<Label>("log-level");
        _messageLabel = this.Q<Label>("log-message");

        _sourceLabel.text = _logEntity.Source;
        _levelLabel.text = _logEntity.Level.ToString();
        _levelLabel.AddToClassList(_logEntity.Level.ToString().ToLowerInvariant());

        _messageLabel.text = _logEntity.FullMessage;
    }
}