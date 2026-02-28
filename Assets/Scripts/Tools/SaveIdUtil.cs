using System;
using UnityEngine;

public static class SaveIdUtil
{
#if UNITY_EDITOR
    public static void EnsureId(ref string id, UnityEngine.Object context)
    {
        if (!string.IsNullOrWhiteSpace(id)) return;
        id = Guid.NewGuid().ToString("N");
        UnityEditor.EditorUtility.SetDirty(context);
    }
#else
    public static void EnsureId(ref string id, UnityEngine.Object context)
    {
        if (!string.IsNullOrWhiteSpace(id)) return;
        id = Guid.NewGuid().ToString("N");
    }
#endif
}