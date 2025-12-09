using UnityEditor;

namespace YesPatchFrameworkForVRChatSdk.Extensions;

internal static class AssetDatabaseExtenstion
{
    public static T? LoadAssetFromGuid<T>(string guid) where T : UnityEngine.Object
    {
        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
            return null;

        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
}