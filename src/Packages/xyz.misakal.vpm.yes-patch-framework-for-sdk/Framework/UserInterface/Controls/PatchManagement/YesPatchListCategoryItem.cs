using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.Extensions;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Controls.PatchManagement;

internal sealed class YesPatchListCategoryItem : VisualElement
{
    private readonly Label _categoryHeaderLabel;
    
    private const string VisualTreeAssetGuid = "c702a10920184a85badf7c61b197ff70";

    public YesPatchListCategoryItem(string categoryName)
    {
        var tree = AssetDatabaseExtenstion.LoadAssetFromGuid<VisualTreeAsset>(VisualTreeAssetGuid);
        if (tree == null)
            throw new FileNotFoundException(
                $"Failed to load YesPatchListCategoryItem UXML asset: YesPatchListCategoryItem.uxml ({VisualTreeAssetGuid})");
        tree.CloneTree(this);

        _categoryHeaderLabel = this.Q<Label>("patch-category-header-text");

        _categoryHeaderLabel.text = categoryName;
        _categoryHeaderLabel.tooltip = categoryName;
    }
}