using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YesPatchFrameworkForVRChatSdk.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.Controls.PatchManagement;
using YesPatchFrameworkForVRChatSdk.UserInterface.StateManagement;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Views.PatchManagement
{
    internal class YesPatchManagementWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        private readonly YesPatchManagerStateManager _patchManagerStateManager = YesPatchManagerStateManager.Instance;
        private readonly YesPatchManager _yesPatchManager = YesPatchManager.Instance;

        [MenuItem("Window/Yes! Patch Framework/Patch Management")]
        public static void ShowPatchManagementWindow()
        {
            var window = GetWindow<YesPatchManagementWindow>();
            window.titleContent = new GUIContent("Patch Management");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            m_VisualTreeAsset.CloneTree(root);

            var patchesListView = root.Q<ListView>("patches-list-view");

            var patches = _yesPatchManager.Patches.ToList();
            patchesListView.makeItem = () => new VisualElement();
            patchesListView.bindItem = (view, index) =>
            {
                var item = new YesPatchListItem(patches[index], _patchManagerStateManager);
                view.Add(item);
            };

            patchesListView.fixedItemHeight = 50;
            patchesListView.itemsSource = patches;
        }
    }
}