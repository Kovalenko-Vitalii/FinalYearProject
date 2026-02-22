using System.Collections.Generic;
using UnityEngine;

public class PlayerTickSystem : MonoBehaviour
{
    private const string TAG = "TickSystem";
    public static PlayerTickSystem Instance { get; private set; }

    // List of actions to trigger each tick and for lateupdate too
    private readonly List<IPlayerTick> ticks = new();
    private readonly List<IPlayerLateTick> lateTicks = new();

    private bool _enabled;

    private void Awake()
    {
        GameLog.Log(TAG, $"Awake() initiated");

        if (Instance != null && Instance != this) {
            GameLog.Warning(TAG, $"Duplicate -> destroy id={GetInstanceID()}");
            Destroy(gameObject); 
            return; 
        }
        Instance = this;

        GameLog.Log(TAG, $"Awake() finished. Singleton set");
    }

    public void SetEnabled(bool enabled) => _enabled = enabled;

    // Adding tick
    public void Register(object obj)
    {
        if (obj is IPlayerTick t && !ticks.Contains(t)) ticks.Add(t);
        if (obj is IPlayerLateTick lt && !lateTicks.Contains(lt)) lateTicks.Add(lt);
    }

    // Removing tick
    public void Unregister(object obj)
    {
        if (obj is IPlayerTick t) ticks.Remove(t);
        if (obj is IPlayerLateTick lt) lateTicks.Remove(lt);
    }

    // Ticking in update and in lateUpdate
    private void Update()
    {
        if (!_enabled) return;
        float dt = Time.deltaTime;
        for (int i = 0; i < ticks.Count; i++)
            ticks[i].Tick(dt);
    }

    private void LateUpdate() 
    {
        if (!_enabled) return;
        float dt = Time.deltaTime;
        for (int i = 0; i < lateTicks.Count; i++)
            lateTicks[i].LateTick(dt);
    }
}

// Interfaces for managers to use
public interface IPlayerTick
{
    void Tick(float dt);
}

public interface IPlayerLateTick
{
    void LateTick(float dt);
}
