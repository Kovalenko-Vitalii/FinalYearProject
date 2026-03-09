using UnityEngine;

// This class represents rabbit idle state
// Sitting on the ground and moving head
public class RabbitIdleState : IAIState
{
    readonly RabbitBrain brain;
    float timer; // time to switch to wander state

    public RabbitIdleState(RabbitBrain brain) => this.brain = brain; // Initializing state
    
    public void Enter()
    {
        brain.Mover.Stop(); // stopping movement
        brain.Mover.SetSpeed(brain.Config.walkSpeed); // next movement will be with walk speed
        timer = Random.Range(brain.Config.minIdleTime, brain.Config.maxIdleTime); // setting random time to wander state from range
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