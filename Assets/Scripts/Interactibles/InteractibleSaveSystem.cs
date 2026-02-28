using System.Linq;
using UnityEngine;

public static class InteractibleSaveSystem
{
    public static InteractibleListSave CaptureAll()
    {
        var data = new InteractibleListSave();

        var interactibles = Object.FindObjectsByType<InteractableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        data.interactibles = interactibles
            .Where(o => !string.IsNullOrEmpty(o.Id))
            .Select(o => new InteractibleSave
            {
                id = o.Id,
                isActive = o.IsActive
            })
            .ToList();

        return data;
    }

    public static void RestoreAll(InteractibleListSave data)
    {
        if (data == null) return;

        var interactibles = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        var map = interactibles
            .Where(o => !string.IsNullOrEmpty(o.Id))
            .ToDictionary(o => o.Id, o => o);

        foreach (var s in data.interactibles)
        {
            if (string.IsNullOrEmpty(s.id)) continue;
            if (map.TryGetValue(s.id, out var obstacle))
                obstacle.ApplyStateImmediate(s.isActive);
        }
    }
}
