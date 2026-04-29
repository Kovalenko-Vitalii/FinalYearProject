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
    private MonsterAttackState attackState;
    private MonsterDeadState deadState;

    public bool attackAnimationPlaying { get; private set; }


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
        attackState = new MonsterAttackState(this);
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
            Context.MonsterMemory.RememberNoise(damage.hitPoint, MonsterConfig.hearingRadius);

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

            if (stateMachine.CurrentState != chaseState &&
            stateMachine.CurrentState != attackState)
            {
                stateMachine.SetState(chaseState);
            }
        }
    }

    private void OnNoiseHeard(AINoiseEvent noise)
    {
        if (Context.Health.IsDead)
            return;

        float effectiveRadius = Mathf.Min(MonsterConfig.hearingRadius, noise.Radius);
        if (effectiveRadius <= 0f)
            return;

        Vector3 delta = noise.Position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude > effectiveRadius * effectiveRadius)
            return;

        Context.MonsterMemory.RememberNoise(noise.Position, noise.Radius);

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
    public void GoToAttack()
    {
        if (!Context.Health.IsDead)
            stateMachine.SetState(attackState);
    }

    public void PlayDeathAnimation()
    {
        if (Context.Animator == null)
            return;

        Context.Animator.SetFloat(Animator.StringToHash("Speed"), 0f);
        Context.Animator.SetTrigger(Animator.StringToHash("Die"));
    }

    public void PlayAttackAnimation()
    {
        if (Context.Animator == null)
            return;

        if (attackAnimationPlaying)
            return;

        attackAnimationPlaying = true;
        Context.Animator.SetTrigger(Animator.StringToHash("Attack"));
        Context.Audio?.Play(AISoundType.Attack);
    }

    public void OnAttackHit()
    {
        Transform target = Context.MonsterMemory.CurrentTarget;
        if (target == null)
            return;

        float distance = GetDistanceToTargetSurface(target);
        if (distance > MonsterConfig.attackRange)
            return;

        PlayerDamageReceiver receiver = target.GetComponentInParent<PlayerDamageReceiver>();
        if (receiver == null)
            return;

        Vector3 hitPoint = target.position;
        Collider targetCollider = target.GetComponentInChildren<Collider>();
        if (targetCollider != null)
            hitPoint = targetCollider.ClosestPoint(transform.position);

        receiver.ReceiveHit(new PlayerHitData(
            MonsterConfig.attackDamage,
            gameObject,
            hitPoint
        ));
    }

    public void OnAttackAnimationFinished()
    {
        attackAnimationPlaying = false;
    }

    private float GetDistanceToTargetSurface(Transform target)
    {
        if (target == null)
            return float.PositiveInfinity;

        Vector3 from = transform.position;
        Vector3 to = target.position;

        Collider targetCollider = target.GetComponentInChildren<Collider>();
        if (targetCollider != null)
            to = targetCollider.ClosestPoint(from);

        from.y = 0f;
        to.y = 0f;

        return Vector3.Distance(from, to);
    }

    private Vector3 GetDesiredAttackPosition(Transform target, float desiredDistance)
    {
        Vector3 targetPos = target.position;
        Vector3 away = transform.position - targetPos;
        away.y = 0f;

        if (away.sqrMagnitude < 0.001f)
            away = -target.forward;

        return targetPos + away.normalized * desiredDistance;
    }

    public void PlayFootstepSound() => Context.Audio?.Play(AISoundType.Footstep);
    public void PlayIdleSoundSound() => Context.Audio?.Play(AISoundType.ChaseStart);

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

            //if (Random.value < 0.25f)
            brain.Context.Audio?.Play(AISoundType.Idle);
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

            brain.Context.Audio?.Play(AISoundType.ChaseStart);

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

            brain.Context.Audio?.Play(AISoundType.Alert);

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
            brain.Context.Mover.SetStoppingDistance(0.1f);
            repathTimer = 0f;
        }

        public void Tick(float dt)
        {
            Transform target = brain.Context.MonsterMemory.CurrentTarget;
            if (target == null)
            {
                brain.GoToSearch();
                return;
            }

            float distance = brain.GetDistanceToTargetSurface(target);

            if (distance <= brain.MonsterConfig.attackRange)
            {
                brain.GoToAttack();
                return;
            }

            if (!brain.Context.MonsterMemory.HasSeenRecently(brain.MonsterConfig.lostSightGraceTime))
            {
                brain.GoToSearch();
                return;
            }

            repathTimer -= dt;

            if (repathTimer <= 0f)
            {
                repathTimer = brain.MonsterConfig.chaseRepathInterval;

                Vector3 desiredPos = brain.GetDesiredAttackPosition(
                    target,
                    brain.MonsterConfig.attackRange * 0.9f);

                brain.Context.Mover.MoveTo(desiredPos);
            }
        }

        public void Exit() { }
    }

    private sealed class MonsterAttackState : IAIState
    {
        private readonly MonsterBrain brain;
        private float cooldown;

        public MonsterAttackState(MonsterBrain brain) => this.brain = brain;

        public void Enter()
        {
            brain.Context.Mover.Stop();
            cooldown = 0f;
        }

        public void Tick(float dt)
        {
            Transform target = brain.Context.MonsterMemory.CurrentTarget;
            if (target == null)
            {
                brain.GoToSearch();
                return;
            }

            float distance = brain.GetDistanceToTargetSurface(target);

            if (distance > brain.MonsterConfig.attackLoseRange)
            {
                brain.GoToChase();
                return;
            }

            brain.Context.Mover.Stop();
            brain.Context.Mover.FaceTowards(target.position);

            cooldown -= dt;

            if (cooldown > 0f)
                return;

            if (brain.attackAnimationPlaying)
                return;

            cooldown = brain.MonsterConfig.attackCooldown;
            brain.PlayAttackAnimation();
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
            brain.Context.Audio?.Play(AISoundType.Death);
        }

        public void Tick(float dt) { }

        public void Exit() { }
    }
}