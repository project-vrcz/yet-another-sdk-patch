using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Views.PatchManagement
{
    internal class YesPatchManagementWindow : EditorWindow
    {
        [MenuItem(FrameworkMenuItem.FrameworkWindows + "Patch Management")]
        [MenuItem(FrameworkMenuItem.ToolsMenuRoot + "Open Patch Management Window")]
        public static void ShowPatchManagementWindow()
        {
            var window = GetWindow<YesPatchManagementWindow>();
            window.titleContent = new GUIContent("Patch Management");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            var view = new YesPatchManagementView
            {
                style =
                {
                    height = new StyleLength(Length.Percent(100))
                }
            };

            root.Add(view);
        }
    }
}