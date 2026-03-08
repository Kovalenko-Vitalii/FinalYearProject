using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class SaveIdUtil
{
#if UNITY_EDITOR
    public static void EnsureId<T>(ref string id, T self) where T : Component
    {
        if (PrefabUtility.IsPartOfPrefabAsset(self))
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                id = "";
                EditorUtility.SetDirty(self);
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            AssignNewId(ref id, self);
            return;
        }

        var all = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var other in all)
        {
            if (other == null || other == self) continue;
            if (other.gameObject.scene != self.gameObject.scene) continue;

            var otherIdField = GetIdUnsafe(other);
            if (!string.IsNullOrWhiteSpace(otherIdField) && otherIdField == id)
            {
                AssignNewId(ref id, self);
                return;
            }
        }
    }
    private static string GetIdUnsafe(Component c)
    {
        var so = new SerializedObject(c);
        var prop = so.FindProperty("id");
        return prop != null ? prop.stringValue : null;
    }

    private static void AssignNewId<T>(ref string id, T self) where T : Component
    {
        id = Guid.NewGuid().ToString("N");
        EditorUtility.SetDirty(self);

        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(self.gameObject.scene);
    }
#else
    public static void EnsureUniqueId<T>(ref string id, T self) where T : Component
    {
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
    }
#endif
}