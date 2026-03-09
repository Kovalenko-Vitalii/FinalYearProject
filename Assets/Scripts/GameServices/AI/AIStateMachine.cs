// This class represents state machine that keeps current state and ticks it
public class AIStateMachine
{
    public IAIState CurrentState { get; private set; }

    // When setting new state we call exit() for previous state
    // and switch current also triggering enter() for new one
    public void SetState(IAIState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    public void Tick(float dt) => CurrentState?.Tick(dt);
        
}

// This interface should be used for each AI state that gonna be added
public interface IAIState
{
    void Enter();
    void Tick(float dt);
    void Exit();
}