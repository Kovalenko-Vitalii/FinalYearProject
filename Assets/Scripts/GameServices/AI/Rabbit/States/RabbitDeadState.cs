// This class represents dead state for rabbit
// Just playing death animation for now
public class RabbitDeadState : IAIState
{
    private readonly RabbitBrain brain;

    public RabbitDeadState(RabbitBrain brain) => this.brain = brain; // Initializing state

    public void Enter()
    {
        brain.Mover.Stop(); // stopping movement
        brain.Mover.Disable(); // disabling movement
        brain.PlayDeathAnimation(); // playing death anim
    }

    public void Tick(float dt) { }

    public void Exit() { }
}