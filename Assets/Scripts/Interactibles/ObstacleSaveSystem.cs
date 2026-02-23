using System.Linq;
using UnityEngine;

public static class ObstacleSaveSystem
{
    public static ObstacleListSave CaptureAll()
    {
        var data = new ObstacleListSave();

        var obstacles = Object.FindObjectsByType<ObjectInteractible>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        data.obstacles = obstacles
            .Where(o => !string.IsNullOrEmpty(o.Id))
            .Select(o => new ObstacleSave
            {
                id = o.Id,
                isActive = o.IsActive
            })
            .ToList();

        return data;
    }

    public static void RestoreAll(ObstacleListSave data)
    {
        if (data == null) return;

        var obstacles = Object.FindObjectsByType<ObjectInteractible>(FindObjectsSortMode.None);
        var map = obstacles
            .Where(o => !string.IsNullOrEmpty(o.Id))
            .ToDictionary(o => o.Id, o => o);

        foreach (var s in data.obstacles)
        {
            if (string.IsNullOrEmpty(s.id)) continue;
            if (map.TryGetValue(s.id, out var obstacle))
                obstacle.ApplyStateImmediate(s.isActive);
        }
    }
}
