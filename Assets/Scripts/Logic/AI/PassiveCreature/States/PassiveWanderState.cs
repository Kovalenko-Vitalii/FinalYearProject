using UnityEngine;

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