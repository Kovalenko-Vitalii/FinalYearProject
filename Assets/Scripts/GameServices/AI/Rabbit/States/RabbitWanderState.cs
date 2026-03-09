using UnityEngine;

// This class represents reabbit wander state
// Rabbit going so some point
public class RabbitWanderState : IAIState
{
    private readonly RabbitBrain brain;

    public RabbitWanderState(RabbitBrain brain) => this.brain = brain; // Initializing state

    public void Enter()
    {
        brain.Mover.SetSpeed(brain.Config.walkSpeed); // setting speed to walling
        Vector3 point = brain.NavPointProvider.GetRandomPoint(brain.transform.position, brain.Config.wanderRadius); // getting random point
        brain.Mover.MoveTo(point); // asking mover to move to the point
    }

    public void Tick(float dt)
    {
        if (brain.Mover.HasReachedDestination()) // if point reached switch to idle
            brain.GoToIdle();
    }

    public void Exit() { }
}