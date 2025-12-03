using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.Controls.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Views.PatchManagement;

internal sealed class YesPatchManagementView : VisualElement
{
    private readonly YesPatchManagerStateManager _patchManagerStateManager = YesPatchManagerStateManager.Instance;
    private readonly YesPatchManager _yesPatchManager = YesPatchManager.Instance;

    public YesPatchManagementView()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/xyz.misakal.vpm.yes-patch-framework-for-sdk/Framework/UserInterface/Views/PatchManagement/" +
            nameof(YesPatchManagementView) + ".uxml");
        tree.CloneTree(this);

        var patchesListView = this.Q<ListView>("patches-list-view");

        var patches = _yesPatchManager.Patches.ToList();
        patchesListView.makeItem = () => new VisualElement();
        patchesListView.bindItem = (view, index) =>
        {
            var item = new YesPatchListItem(patches[index], _patchManagerStateManager);
            view.Add(item);
        };

        patchesListView.fixedItemHeight = 67;
        patchesListView.itemsSource = patches;
    }
}