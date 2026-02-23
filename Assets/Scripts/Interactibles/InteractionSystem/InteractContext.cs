public sealed class InteractContext
{
    public ObjectInteractible interactable;
    public PlayerInteractor interactor;

    public InteractContext(ObjectInteractible interactable, PlayerInteractor interactor)
    {
        this.interactable = interactable;
        this.interactor = interactor;
    }
}