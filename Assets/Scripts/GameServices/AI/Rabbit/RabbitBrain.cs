using UnityEngine;

// This class is the main logic manager for rabbit AI
public class RabbitBrain : MonoBehaviour, IDamageable, IPlayerTick
{
    string TAG = "RabbitBrain";
    [field: SerializeField] public RabbitConfig Config { get; private set; }
    [field: SerializeField] public AIContext Context { get; private set; }
    [field: SerializeField] public AIMover Mover { get; private set; }
    [field: SerializeField] public AINavPointProvider NavPointProvider { get; private set; }

    // State machine and states
    AIStateMachine stateMachine;
    RabbitIdleState idleState;
    RabbitWanderState wanderState;
    RabbitFleeState fleeState;
    RabbitDeadState deadState;

    // Values for animator
    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int DieHash = Animator.StringToHash("Die");

    void OnEnable() => PlayerTickSystem.Instance?.Register(this);
    void OnDisable() => PlayerTickSystem.Instance?.Unregister(this);

    private void Reset()
    {
        Context = GetComponent<AIContext>();
        Mover = GetComponent<AIMover>();
        NavPointProvider = GetComponent<AINavPointProvider>();
    }

    private void Awake()
    {
        ValidateDependencies();

        stateMachine = new AIStateMachine();

        idleState = new RabbitIdleState(this);
        wanderState = new RabbitWanderState(this);
        fleeState = new RabbitFleeState(this);
        deadState = new RabbitDeadState(this);

        Context.Health.Initialize(Config.maxHp); // setting hp
        Context.Health.Died += OnDied; // subscribing for died action
    }

    private void OnDestroy()
    {
        if (Context != null && Context.Health != null)
            Context.Health.Died -= OnDied;
    }

    private void Start()
    {
        GoToIdle();
    }

    public void Tick(float dt)
    {
        if (!Context.Health.IsDead)
            UpdateAnimatorAlive();

        stateMachine.Tick(dt);
    }

    public void GoToIdle()
    {
        if (!Context.Health.IsDead)
            stateMachine.SetState(idleState);
    }

    public void GoToWander()
    {
        if (!Context.Health.IsDead)
            stateMachine.SetState(wanderState);
    }

    public void GoToFlee()
    {
        if (!Context.Health.IsDead)
            stateMachine.SetState(fleeState);
    }

    public void TakeDamage(int damage, Vector3 hitPoint, GameObject source)
    {
        if (Context.Health.IsDead) // if already dead ignoring
            return;

        Context.ThreatMemory.RememberThreat(source, hitPoint); // remembering who attacked
        Context.Health.ApplyDamage(damage); // applying damage

        if (!Context.Health.IsDead)
            GoToFlee(); // if not dead start fleeing
    }

    private void OnDied()
    {
        stateMachine.SetState(deadState);
    }

    // Correcting speed of animation according to AI move speed
    private void UpdateAnimatorAlive()
    {
        if (Context.Animator == null)
            return;

        Context.Animator.SetFloat(SpeedHash, Mover.VelocityMagnitude());
    }

    public void PlayDeathAnimation()
    {
        if (Context.Animator == null)
            return;

        Context.Animator.SetFloat(SpeedHash, 0f);
        Context.Animator.SetTrigger(DieHash);
    }

    // Warn if something wrong
    private void ValidateDependencies()
    {
        if (Config == null)
            GameLog.Error(TAG, "RabbitConfig is missing");

        if (Context == null)
            GameLog.Error(TAG, "AIContext is missing");

        if (Mover == null)
            GameLog.Error(TAG, "AIMover is missing");

        if (NavPointProvider == null)
            GameLog.Error(TAG, "AINavPointProvider is missing");

        if (Context != null)
        {
            if (Context.Agent == null)
                GameLog.Error(TAG, "NavMeshAgent is missing in AIContext");

            if (Context.Animator == null)
                GameLog.Error(TAG, "Animator is missing in AIContext");

            if (Context.Health == null)
                GameLog.Error(TAG, "AIHealth is missing in AIContext");

            if (Context.ThreatMemory == null)
                GameLog.Error(TAG, "AIThreatMemory is missing in AIContext");
        }
    }
}