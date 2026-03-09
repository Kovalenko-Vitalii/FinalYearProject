using UnityEngine;

// This class represents rabbit flee state from attacker
public class RabbitFleeState : IAIState
{
    readonly RabbitBrain brain;
    float timer; // time to flee
    float repathTimer; // time to change path

    public RabbitFleeState(RabbitBrain brain) => this.brain = brain; // Initializing state

    public void Enter()
    {
        brain.Mover.SetSpeed(brain.Config.runSpeed); // setting speed
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
        Vector3 target = brain.NavPointProvider.GetRandomPoint(rawTarget, 3f);
        brain.Mover.MoveTo(target);
    }
}