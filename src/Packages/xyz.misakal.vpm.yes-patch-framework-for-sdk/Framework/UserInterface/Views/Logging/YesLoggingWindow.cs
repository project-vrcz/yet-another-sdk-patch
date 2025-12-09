using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YesPatchFrameworkForVRChatSdk.UserInterface.Views.Logging
{
    internal sealed class YesLoggingWindow : EditorWindow
    {
        [MenuItem("Window/Yes! Patch Framework/Logging")]
        public static void ShowPatchManagementWindow()
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