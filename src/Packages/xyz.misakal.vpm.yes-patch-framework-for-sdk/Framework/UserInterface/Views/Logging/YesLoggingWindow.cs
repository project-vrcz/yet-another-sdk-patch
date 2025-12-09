using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Views.Logging
{
    internal sealed class YesLoggingWindow : EditorWindow
    {
        [MenuItem(FrameworkMenuItem.FrameworkWindows + "Logging")]
        [MenuItem(FrameworkMenuItem.ToolsMenuRoot + "Open Logging Window")]
        public static void ShowLoggingWindow()
        {
            var window = GetWindow<YesLoggingWindow>();
            window.titleContent = new GUIContent("Yes! Logging");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            var view = new YesLoggingView
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