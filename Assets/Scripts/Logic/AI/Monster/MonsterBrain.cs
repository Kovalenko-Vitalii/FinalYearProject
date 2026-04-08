using UnityEngine;

public class MonsterBrain : AgentBrain
{
    [SerializeField] private Transform explicitTarget;
    [SerializeField] private string playerTag = "Player";

    private MonsterAIConfig MonsterConfig => (MonsterAIConfig)Config;
    private Transform target;

    private MonsterIdleState idleState;
    private MonsterWanderState wanderState;
    private MonsterInvestigateState investigateState;
    private MonsterSearchState searchState;
    private MonsterChaseState chaseState;
    private MonsterDeadState deadState;

    protected override void Awake()
    {
        base.Awake();

        if (Config is not MonsterAIConfig)
        {
            Debug.LogError($"{name}: MonsterBrain requires MonsterAIConfig", this);
            enabled = false;
            return;
        }

        if (Context == null ||
            Context.Mover == null ||
            Context.Health == null ||
            Context.NavPointProvider == null ||
            Context.MonsterMemory == null ||
            Context.Vision == null ||
            Context.Hearing == null)
        {
            Debug.LogError($"{name}: MonsterBrain has incomplete AIContext", this);
            enabled = false;
            return;
        }

        idleState = new MonsterIdleState(this);
        wanderState = new MonsterWanderState(this);
        investigateState = new MonsterInvestigateState(this);
        searchState = new MonsterSearchState(this);
        chaseState = new MonsterChaseState(this);
        deadState = new MonsterDeadState(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Context?.Hearing != null)
            Context.Hearing.NoiseHeard += OnNoiseHeard;
    }

    protected override void OnDisable()
    {
        if (Context?.Hearing != null)
            Context.Hearing.NoiseHeard -= OnNoiseHeard;

        base.OnDisable();
    }

    protected override void Start()
    {
        ResolveTarget();
        base.Start();
    }

    protected override void EnterDefaultState()
    {
        GoToIdle();
    }

    public override void Tick(float dt)
    {
        if (!Context.Health.IsDead)
        {
            ResolveTarget();
            UpdatePerception();
        }

        base.Tick(dt);
    }

    public override void TakeDamage(DamageData damage)
    {
        if (Context.Health.IsDead)
            return;

        if (damage.source != null)
            Context.MonsterMemory.RememberSeenTarget(damage.source.transform);
        else
            Context.MonsterMemory.RememberNoise(damage.hitPoint);

        Context.Health.ApplyDamage(damage.amount);

        if (!Context.Health.IsDead)
            stateMachine.SetState(chaseState);
    }

    protected override void OnDied()
    {
        base.OnDied();
        stateMachine.SetState(deadState);
    }

    private void ResolveTarget()
    {
        if (explicitTarget != null)
        {
            target = explicitTarget;
            return;
        }

        if (target != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            target = player.transform;
    }

    private void UpdatePerception()
    {
        if (target == null)
            return;

        if (Context.Vision.CanSee(
                target,
                MonsterConfig.visionRange,
                MonsterConfig.viewAngle,
                MonsterConfig.eyeHeight,
                MonsterConfig.visionBlockers))
        {
            Context.MonsterMemory.RememberSeenTarget(target);

            if (stateMachine.CurrentState != chaseState)
                stateMachine.SetState(chaseState);
        }
    }

    private void OnNoiseHeard(Vector3 noisePosition)
    {
        if (Context.Health.IsDead)
            return;

        Vector3 delta = noisePosition - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude > MonsterConfig.hearingRadius * MonsterConfig.hearingRadius)
            return;

        Context.MonsterMemory.RememberNoise(noisePosition);

        if (stateMachine.CurrentState != chaseState)
            stateMachine.SetState(investigateState);
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

    public void GoToInvestigate()
    {
        if (!Context.Health.IsDead)
            stateMachine.SetState(investigateState);
    }

    public void GoToSearch()
    {
        if (!Context.Health.IsDead)
            stateMachine.SetState(searchState);
    }

    public void GoToChase()
    {
        if (!Context.Health.IsDead)
            stateMachine.SetState(chaseState);
    }

    public void PlayDeathAnimation()
    {
        if (Context.Animator == null)
            return;

        Context.Animator.SetFloat(Animator.StringToHash("Speed"), 0f);
        Context.Animator.SetTrigger(Animator.StringToHash("Die"));
    }

    private sealed class MonsterIdleState : IAIState
    {
        private readonly MonsterBrain brain;
        private float timer;

        public MonsterIdleState(MonsterBrain brain) => this.brain = brain;

        public void Enter()
        {
            brain.Context.Mover.Stop();
            brain.Context.Mover.SetSpeed(brain.MonsterConfig.walkSpeed);

            timer = Random.Range(
                brain.MonsterConfig.minIdleTime,
                brain.MonsterConfig.maxIdleTime);

            if (Random.value < 0.25f)
                brain.Context.Audio?.PlayIdle();
        }

        public void Tick(float dt)
        {
            timer -= dt;

            if (timer <= 0f)
                brain.GoToWander();
        }

        public void Exit() { }
    }

    private sealed class MonsterWanderState : IAIState
    {
        private readonly MonsterBrain brain;

        public MonsterWanderState(MonsterBrain brain) => this.brain = brain;

        public void Enter()
        {
            brain.Context.Mover.SetSpeed(brain.MonsterConfig.walkSpeed);

            Vector3 point = brain.Context.NavPointProvider.GetRandomPoint(
                brain.transform.position,
                brain.MonsterConfig.wanderRadius);

            brain.Context.Mover.MoveTo(point);
        }

        public void Tick(float dt)
        {
            if (brain.Context.Mover.HasReachedDestination())
                brain.GoToIdle();
        }

        public void Exit() { }
    }

    private sealed class MonsterInvestigateState : IAIState
    {
        private readonly MonsterBrain brain;

        public MonsterInvestigateState(MonsterBrain brain) => this.brain = brain;

        public void Enter()
        {
            brain.Context.Mover.SetSpeed(brain.MonsterConfig.walkSpeed);
            brain.Context.Mover.MoveTo(brain.Context.MonsterMemory.LastHeardPosition);
        }

        public void Tick(float dt)
        {
            if (brain.Context.Mover.HasReachedDestination())
                brain.GoToSearch();
        }

        public void Exit() { }
    }

    private sealed class MonsterSearchState : IAIState
    {
        private readonly MonsterBrain brain;
        private float timer;
        private float repathTimer;
        private Vector3 center;

        public MonsterSearchState(MonsterBrain brain) => this.brain = brain;

        public void Enter()
        {
            brain.Context.Mover.SetSpeed(brain.MonsterConfig.walkSpeed);

            center = brain.Context.MonsterMemory.HasSeenTarget
                ? brain.Context.MonsterMemory.LastKnownTargetPosition
                : brain.Context.MonsterMemory.LastHeardPosition;

            timer = brain.MonsterConfig.searchDuration;
            repathTimer = 0f;
        }

        public void Tick(float dt)
        {
            timer -= dt;
            repathTimer -= dt;

            if (timer <= 0f)
            {
                brain.GoToIdle();
                return;
            }

            if (repathTimer <= 0f || brain.Context.Mover.HasReachedDestination())
            {
                repathTimer = brain.MonsterConfig.searchRepathInterval;

                Vector3 point = brain.Context.NavPointProvider.GetRandomPoint(
                    center,
                    brain.MonsterConfig.searchRadius);

                brain.Context.Mover.MoveTo(point);
            }
        }

        public void Exit() { }
    }

    private sealed class MonsterChaseState : IAIState
    {
        private readonly MonsterBrain brain;
        private float repathTimer;

        public MonsterChaseState(MonsterBrain brain) => this.brain = brain;

        public void Enter()
        {
            brain.Context.Mover.SetSpeed(brain.MonsterConfig.chaseSpeed);
            repathTimer = 0f;
        }

        public void Tick(float dt)
        {
            if (!brain.Context.MonsterMemory.HasSeenRecently(brain.MonsterConfig.lostSightGraceTime))
            {
                brain.GoToSearch();
                return;
            }

            repathTimer -= dt;

            if (repathTimer <= 0f)
            {
                repathTimer = brain.MonsterConfig.chaseRepathInterval;

                Vector3 targetPoint = brain.Context.MonsterMemory.CurrentTarget != null
                    ? brain.Context.MonsterMemory.CurrentTarget.position
                    : brain.Context.MonsterMemory.LastKnownTargetPosition;

                brain.Context.Mover.MoveTo(targetPoint);
            }
        }

        public void Exit() { }
    }

    private sealed class MonsterDeadState : IAIState
    {
        private readonly MonsterBrain brain;

        public MonsterDeadState(MonsterBrain brain) => this.brain = brain;

        public void Enter()
        {
            brain.Context.Mover.Stop();
            brain.Context.Mover.Disable();
            brain.PlayDeathAnimation();
        }

        public void Tick(float dt) { }

        public void Exit() { }
    }
}