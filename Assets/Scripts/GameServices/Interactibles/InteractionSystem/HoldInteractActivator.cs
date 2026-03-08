using UnityEngine;

public class HoldInteractActivator : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback
{
    [SerializeField] private InteractExecutor executor;

    [Header("Hold")]
    [SerializeField] private float holdDuration = 1.0f;

    [Header("Execution Policy")]
    [SerializeField] private ExecutePolicy policy = ExecutePolicy.Default;

    private void Awake()
    {
        if (executor == null)
            executor = GetComponent<InteractExecutor>();
    }

    public float GetInteractDuration(PlayerInteractor interactor)
    {
        if (executor == null) return 0f;
        return executor.CanExecute(policy) ? holdDuration : 0f;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if (executor == null) return false;
        var ctx = new InteractContext(executor, interactor.gameObject, interactor);
        return executor.Execute(ctx, policy);
    }

    public void OnHoldCanceled(PlayerInteractor interactor) { }
    public void OnHoldStart(PlayerInteractor interactor, float duration) { }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = "";
        if (executor == null) return false;

        if (!executor.TryGetPrompt(out var basePrompt, policy)) return false;

        if (executor.CanExecute(policy))
            prompt = basePrompt + " Hold to interact.";
        else
            prompt = basePrompt;

        return true;
    }
}