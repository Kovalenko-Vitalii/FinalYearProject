using UnityEngine;

public class StatsTicker : MonoBehaviour, IPlayerTick
{
    private void Start()
    {
        PlayerTickSystem.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (PlayerTickSystem.Instance != null)
            PlayerTickSystem.Instance.Unregister(this);
    }

    public void Tick(float dt)
    {
        var stats = PlayerStatManager.Instance;
        var effects = StatusEffectManager.Instance;
        if (stats == null || effects == null) return;

        effects.TickEffects(dt, stats);
        var snapshot = effects.BuildSnapshot(stats);
        stats.TickNaturalStats(dt, snapshot);
    }
}

