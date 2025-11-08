using UnityEngine;

[RequireComponent(typeof(WorldContainer))]
public class WorldContainerInteractable : MonoBehaviour, IInteractable
{
    [Header("UX")]
    [SerializeField] private string displayName = "Box";

    private WorldContainer container;

    private void Awake()
    {
        container = GetComponent<WorldContainer>();
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = $"Open {displayName}";
        return true;
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

        return true;
    }
}
