using System.Collections.Generic;
using UnityEngine;

public interface IPlayerTick
{
    void Tick(float dt);
}

public interface IPlayerLateTick
{
    void LateTick(float dt);
}

public class PlayerTickSystem : MonoBehaviour
{
    public static PlayerTickSystem Instance { get; private set; }

    private readonly List<IPlayerTick> _ticks = new();
    private readonly List<IPlayerLateTick> _lateTicks = new();

    private bool _enabled;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetEnabled(bool enabled) => _enabled = enabled;

    public void Register(object obj)
    {
        if (obj is IPlayerTick t && !_ticks.Contains(t)) _ticks.Add(t);
        if (obj is IPlayerLateTick lt && !_lateTicks.Contains(lt)) _lateTicks.Add(lt);
    }

    public void Unregister(object obj)
    {
        if (obj is IPlayerTick t) _ticks.Remove(t);
        if (obj is IPlayerLateTick lt) _lateTicks.Remove(lt);
    }

    private void Update()
    {
        if (!_enabled) return;
        float dt = Time.deltaTime;
        for (int i = 0; i < _ticks.Count; i++)
            _ticks[i].Tick(dt);
    }

    private void LateUpdate()
    {
        if (!_enabled) return;
        float dt = Time.deltaTime;
        for (int i = 0; i < _lateTicks.Count; i++)
            _lateTicks[i].LateTick(dt);
    }
}
