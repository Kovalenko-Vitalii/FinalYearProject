using System.Linq;
using UnityEngine;

public static class DoorSaveSystem
{
    public static SaveDoorsData CaptureAll()
    {
        var data = new SaveDoorsData();
        var doors = Object.FindObjectsByType<DoorInteractable>(FindObjectsSortMode.None);

        data.doors = doors
            .Where(d => !string.IsNullOrEmpty(d.Id))
            .Select(d => new DoorSave
            {
                id = d.Id,
                isOpen = d.IsOpen,
                isLocked = d.IsLocked
            })
            .ToList();

        return data;
    }

    public static void RestoreAll(SaveDoorsData data)
    {
        if (data == null) return;

        var doors = Object.FindObjectsByType<DoorInteractable>(FindObjectsSortMode.None);
        var map = doors.Where(d => !string.IsNullOrEmpty(d.Id))
                       .ToDictionary(d => d.Id, d => d);

        foreach (var s in data.doors)
            if (!string.IsNullOrEmpty(s.id) && map.TryGetValue(s.id, out var door))
                door.ApplyStateImmediate(s.isOpen, s.isLocked);
    }
}
