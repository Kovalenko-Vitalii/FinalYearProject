public interface IInteractable
{
    bool TryGetPrompt(PlayerInteractor interactor, out string prompt);

    bool Interact(PlayerInteractor interactor);
}
