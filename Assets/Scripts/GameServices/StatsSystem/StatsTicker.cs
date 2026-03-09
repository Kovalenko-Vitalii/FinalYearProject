using UnityEngine;

public class StatsTicker : MonoBehaviour, IPlayerTick
{
    [SerializeField] private PlayerMovement move;

    private void Awake()
    {
        if (!move) move = GetComponent<PlayerMovement>();
    }

    void OnEnable() => PlayerTickSystem.Instance?.Register(this);
    void OnDisable() => PlayerTickSystem.Instance?.Unregister(this);

    public void Tick(float dt)
    {
        var stats = PlayerStatManager.Instance;
        var effects = StatusEffectManager.Instance;
        if (stats == null || effects == null) return;

        effects.TickEffects(dt);

        bool sprinting = move != null && move.IsSprinting;
        stats.Tick(dt, sprinting);
    }
}
