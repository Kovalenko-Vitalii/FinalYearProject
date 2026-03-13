using UnityEngine;

public abstract class AgentBrain : MonoBehaviour, IDamageable, IPlayerTick
{
    [field: SerializeField] public AIConfig Config { get; protected set; }
    [field: SerializeField] public AIContext Context { get; protected set; }

    // Values for animator
    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int DieHash = Animator.StringToHash("Die");

    protected AIStateMachine stateMachine;

    protected virtual void Awake()
    {
        stateMachine = new AIStateMachine();
        Context.Health.Initialize(Config.maxHp);
        Context.Health.Died += OnDied;
    }

    protected virtual void OnEnable() => PlayerTickSystem.Instance?.Register(this);
    protected virtual void OnDisable() => PlayerTickSystem.Instance?.Unregister(this);

    protected virtual void OnDestroy()
    {
        if (Context != null && Context.Health != null)
            Context.Health.Died -= OnDied;
    }

    protected virtual void Start()
    {
        EnterDefaultState();
    }

    public virtual void Tick(float dt)
    {
        if (!Context.Health.IsDead)
            UpdateAnimatorAlive();

        stateMachine.Tick(dt);
    }

    public virtual void TakeDamage(int damage, Vector3 hitPoint, GameObject source)
    {
        if (Context.Health.IsDead)
            return;

        Context.ThreatMemory.RememberThreat(source, hitPoint);
        Context.Health.ApplyDamage(damage);
    }

    protected virtual void OnDied()
    {
        AIManager.Instance?.NotifyAIDied(this);
        Context.LootDrop?.Drop(Config);
    }

    protected abstract void EnterDefaultState();

    protected virtual void UpdateAnimatorAlive()
    {
        if (Context.Animator == null)
            return;

        Context.Animator.SetFloat(SpeedHash, Context.Mover.VelocityMagnitude());
    }
}
