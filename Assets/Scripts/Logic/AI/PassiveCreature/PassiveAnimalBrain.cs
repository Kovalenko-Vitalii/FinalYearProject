using UnityEngine;

// This class is the main logic manager for passive AI
public class PassiveAnimalBrain : AgentBrain
{
    PassiveIdleState idleState;
    PassiveWanderState wanderState;
    PassiveFleeState fleeState;
    PassiveDeadState deadState;

    protected override void Awake()
    {
        base.Awake();

        if (!enabled)
            return;

        idleState = new PassiveIdleState(this);
        wanderState = new PassiveWanderState(this);
        fleeState = new PassiveFleeState(this);
        deadState = new PassiveDeadState(this);
    }

    protected override void EnterDefaultState()
    {
        GoToIdle();
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

    public override void TakeDamage(DamageData damage)
    {
        base.TakeDamage(damage);

        Context.Audio?.Play(AISoundType.Hurt);

        if (!Context.Health.IsDead)
            GoToFlee();
    }

    protected override void OnDied()
    {
        base.OnDied();
        stateMachine.SetState(deadState);
    }

    public void PlayDeathAnimation()
    {
        if (Context.Animator == null)
            return;

        Context.Audio?.Play(AISoundType.Death);

        Context.Animator.SetFloat(Animator.StringToHash("Speed"), 0f);
        Context.Animator.SetTrigger(Animator.StringToHash("Die"));
    }

    public void PlayFootstepSound() => Context.Audio?.Play(AISoundType.Footstep);
}

// This class represents passive animal wander state
// Rabbit going so some point
public class PassiveWanderState : IAIState
{
    private readonly PassiveAnimalBrain brain;

    public PassiveWanderState(PassiveAnimalBrain brain) => this.brain = brain; // Initializing state

    public void Enter()
    {
        brain.Context.Mover.SetSpeed(brain.Config.walkSpeed); // setting speed to walling
        Vector3 point = brain.Context.NavPointProvider.GetRandomPoint(brain.transform.position, brain.Config.wanderRadius); // getting random point
        brain.Context.Mover.MoveTo(point); // asking mover to move to the point
    }

    public void Tick(float dt)
    {
        if (brain.Context.Mover.HasReachedDestination()) // if point reached switch to idle
            brain.GoToIdle();
    }

    public void Exit() { }
}

// This class represents passove animal idle state
// Sitting on the ground and moving head
public class PassiveIdleState : IAIState
{
    readonly PassiveAnimalBrain brain;
    float timer; // time to switch to wander state

    public PassiveIdleState(PassiveAnimalBrain brain) => this.brain = brain; // Initializing state

    public void Enter()
    {
        brain.Context.Mover.Stop(); // stopping movement
        brain.Context.Mover.SetSpeed(brain.Config.walkSpeed); // next movement will be with walk speed
        timer = Random.Range(brain.Config.minIdleTime, brain.Config.maxIdleTime); // setting random time to wander state from range

        //if (Random.value < 0.35f)
        brain.Context.Audio?.Play(AISoundType.Idle);
    }

    // Ticking time and switching state if needed
    public void Tick(float dt)
    {
        timer -= dt;

        if (timer <= 0f)
            brain.GoToWander();
    }

    public void Exit() { }

}

// This class represents passive animal flee state from attacker
public class PassiveFleeState : IAIState
{
    readonly PassiveAnimalBrain brain;
    float timer; // time to flee
    float repathTimer; // time to change path

    public PassiveFleeState(PassiveAnimalBrain brain) => this.brain = brain; // Initializing state

    public void Enter()
    {
        brain.Context.Mover.SetSpeed(brain.Config.runSpeed); // setting speed
        timer = brain.Config.fleeDuration; // getting flee time
        repathTimer = 0f; // resetting timer so next tick rabbit will receive new path point
        Repath();
    }

    public void Tick(float dt)
    {
        timer -= dt; // ticking timers
        repathTimer -= dt;

        if (repathTimer <= 0f) // changing path and setting new time
        {
            repathTimer = brain.Config.fleeRepathInterval;
            Repath();
        }

        if (timer <= 0f)
            brain.GoToIdle();
    }

    public void Exit() { }

    private void Repath()
    {
        Vector3 dangerPos = brain.Context.ThreatMemory.LastThreatPosition; // getting point to run from
        Vector3 dir = (brain.transform.position - dangerPos).normalized; // getting dierction for movement

        // !!! need to firure this out
        if (dir.sqrMagnitude < 0.001f)
            dir = Random.insideUnitSphere.normalized;

        dir.y = 0f;

        Vector3 rawTarget = brain.transform.position + dir * brain.Config.fleeDistance;
        Vector3 target = brain.Context.NavPointProvider.GetRandomPoint(rawTarget, 3f);
        brain.Context.Mover.MoveTo(target);
    }
}

// This class represents dead state for passive animal
// Just playing death animation for now
public class PassiveDeadState : IAIState
{
    private readonly PassiveAnimalBrain brain;

    public PassiveDeadState(PassiveAnimalBrain brain) => this.brain = brain; // Initializing state

    public void Enter()
    {
        brain.Context.Mover.Stop(); // stopping movement
        brain.Context.Mover.Disable(); // disabling movement
        brain.PlayDeathAnimation(); // playing death anim
    }

    public void Tick(float dt) { }

    public void Exit() { }
}