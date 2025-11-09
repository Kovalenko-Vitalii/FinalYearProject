using UnityEngine;

[RequireComponent(typeof(WorldContainer))]
public class WorldContainerInteractable : MonoBehaviour, IInteractable, IHoldInteractable
{
    [Header("UX")]
    [SerializeField] private string displayName = "Box";

    [Header("Search Settings")]
    [SerializeField] private float firstSearchDuration = 1.5f;
    [SerializeField] private float quickOpenDuration = 0.1f;

    private WorldContainer container;
    private bool isSearched = false;

    private void Awake()
    {
        container = GetComponent<WorldContainer>();
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = $"Open {displayName}";
        return true;
    }

    public float GetInteractDuration(PlayerInteractor interactor)
    {
        return isSearched ? quickOpenDuration : firstSearchDuration;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if (container == null)
            return false;

        var ui = CanvasSwitcher.Instance;
        if (ui == null)
            return false;

        var containerRoot = ui.Container.Root;
        var containerUI = containerRoot.GetComponent<ContainerUI>();
        if (containerUI == null)
            return false;

        containerUI.ShowFor(container);
        ui.OpenContainer();

        isSearched = true;

        return true;
    }
}
