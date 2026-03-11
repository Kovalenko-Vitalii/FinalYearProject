using System;
using UnityEngine;

public interface IInteractable
{
    bool TryGetPrompt(PlayerInteractor interactor, out string prompt);

    bool Interact(PlayerInteractor interactor);
}
public interface IHoldInteractable : IInteractable
{
    float GetInteractDuration(PlayerInteractor interactor);
}
public interface IHoldFeedback
{
    void OnHoldStart(PlayerInteractor interactor, float duration);

    void OnHoldCanceled(PlayerInteractor interactor);
}

[Flags]
public enum ExecutePolicy
{
    Default = 0,
    IgnoreRequirements = 1 << 0,
    IgnoreCosts = 1 << 1,
    IgnoreLock = 1 << 2,
}

public sealed class InteractContext
{
    public readonly InteractExecutor executor;
    public readonly GameObject instigator;
    public readonly PlayerInteractor interactor;

    public InteractContext(InteractExecutor executor, GameObject instigator, PlayerInteractor interactor = null)
    {
        this.executor = executor;
        this.instigator = instigator;
        this.interactor = interactor;
    }
}

public abstract class InteractAction : ScriptableObject
{
    public abstract void Execute(InteractContext ctx);
}