using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.Extensions;
using YesPatchFrameworkForVRChatSdk.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.Controls.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Views.PatchManagement;

internal sealed class YesPatchManagementView : VisualElement
{
    private readonly YesPatchManagerStateManager _patchManagerStateManager = YesPatchManagerStateManager.Instance;
    private readonly YesPatchManager _yesPatchManager = YesPatchManager.Instance;

    private readonly VisualElement _contentContainer;
    private readonly VisualElement _welcomePageContainer;

    private VisualElement? _patchSettingsUi;

    private const string VisualTreeAssetGuid = "678da0557d259cb469a82dc821e0c3cb";

    public YesPatchManagementView()
    {
        var tree = AssetDatabaseExtenstion.LoadAssetFromGuid<VisualTreeAsset>(VisualTreeAssetGuid);
        if (tree == null)
            throw new FileNotFoundException(
                $"Failed to load YesPatchManagementView UXML asset: YesPatchManagementView.uxml ({VisualTreeAssetGuid})");
        tree.CloneTree(this);

        _contentContainer = this.Q<VisualElement>("patch-management-content-container");
        _welcomePageContainer = this.Q<VisualElement>("welcome-page-container");

        var patchesListView = this.Q<ListView>("patches-list-view");

        var patchesNavigationRecords = _yesPatchManager.Patches
            .Select(p => new YesPatchNavigationRecord(p))
            .GroupBy(p => p.Patch.Category)
            .SelectMany(g => new YesPatchNavigationRecordBase[]
            {
                new YesPatchNavigationCategoryRecord(g.Key),
            }.Concat(g))
            .ToArray();

        patchesListView.makeItem = () => new VisualElement();
        patchesListView.bindItem = (view, index) =>
        {
            if (patchesNavigationRecords[index] is YesPatchNavigationCategoryRecord categoryRecord)
            {
                view.Add(new YesPatchListCategoryItem(categoryRecord.Text));
                return;
            }

            if (patchesNavigationRecords[index] is not YesPatchNavigationRecord record)
                return;

            var item = new YesPatchListItem(record.Patch, _patchManagerStateManager);
            view.Add(item);
        };

        patchesListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        patchesListView.itemsSource = patchesNavigationRecords;

        patchesListView.selectionType = SelectionType.Single;
        patchesListView.selectionChanged += objects =>
        {
            if (objects.FirstOrDefault(obj => obj is YesPatchNavigationRecord)
                is not YesPatchNavigationRecord selectedPatch)
            {
                patchesListView.ClearSelection();
                return;
            }

            CreateSettingsUi(selectedPatch.Patch);
        };
    }

    private void CreateSettingsUi(YesPatch patch)
    {
        _welcomePageContainer.style.display = new StyleEnum<DisplayStyle>(StyleKeyword.None);

        if (_patchSettingsUi is not null && _contentContainer.Contains(_patchSettingsUi))
            _contentContainer.Remove(_patchSettingsUi);

        _patchSettingsUi = new YesPatchSettingsUi(patch);
        _patchSettingsUi.style.height = new StyleLength(Length.Percent(100));

        _contentContainer.Add(_patchSettingsUi);
    }

    private abstract class YesPatchNavigationRecordBase
    {
    }

    private class YesPatchNavigationCategoryRecord : YesPatchNavigationRecordBase
    {
        public YesPatchNavigationCategoryRecord(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    private sealed class YesPatchNavigationRecord : YesPatchNavigationRecordBase
    {
        public YesPatchNavigationRecord(YesPatch patch)
        {
            Patch = patch;
        }

        public YesPatch Patch { get; }
    }
}