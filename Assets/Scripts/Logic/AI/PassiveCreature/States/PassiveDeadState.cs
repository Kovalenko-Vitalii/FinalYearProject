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