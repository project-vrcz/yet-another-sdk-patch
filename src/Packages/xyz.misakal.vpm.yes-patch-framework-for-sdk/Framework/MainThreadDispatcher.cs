using System;
using System.Collections.Concurrent;
using UnityEditor;

namespace YesPatchFrameworkForVRChatSdk;

internal static class MainThreadDispatcher
{
    private static readonly ConcurrentQueue<Action> Queue = new();

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (Queue.TryDequeue(out var action))
        {
            action();
        }
    }

    public static void Dispatch(Action action)
    {
        Queue.Enqueue(action);
    }
}