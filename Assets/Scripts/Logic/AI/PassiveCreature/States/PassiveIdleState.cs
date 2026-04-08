using UnityEngine;

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

        if (Random.value < 0.35f)
            brain.Context.Audio?.PlayIdle();
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