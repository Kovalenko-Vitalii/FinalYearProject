using System.Collections.Generic;
using UnityEngine;

public class SpawnPointRegistry : MonoBehaviour
{
    public static SpawnPointRegistry Instance { get; private set; }

    private readonly Dictionary<string, PlayerSpawnPoint> _points = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(PlayerSpawnPoint point)
    {
        if (!point || string.IsNullOrEmpty(point.Id)) return;
        _points[point.Id] = point;
    }

    public void Unregister(PlayerSpawnPoint point)
    {
        if (!point) return;
        if (_points.TryGetValue(point.Id, out var current) && current == point)
            _points.Remove(point.Id);
    }

    public Transform Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _points.TryGetValue(id, out var p) ? p.transform : null;
    }
}
