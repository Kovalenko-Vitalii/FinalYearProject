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

    public override void TakeDamage(int damage, Vector3 hitPoint, GameObject source)
    {
        base.TakeDamage(damage, hitPoint, source);

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

        Context.Animator.SetFloat(Animator.StringToHash("Speed"), 0f);
        Context.Animator.SetTrigger(Animator.StringToHash("Die"));
    }
}