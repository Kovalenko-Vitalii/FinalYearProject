public interface IHoldFeedback
{
    void OnHoldStart(PlayerInteractor interactor, float duration);

    void OnHoldCanceled(PlayerInteractor interactor);
}
