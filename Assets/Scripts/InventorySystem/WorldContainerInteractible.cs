using UnityEngine;

[RequireComponent(typeof(WorldContainer))]
public class WorldContainerInteractable : MonoBehaviour, IInteractable
{
    [Header("UX")]
    [SerializeField] private string displayName = "Box";

    private WorldContainer container;
    private Transform player;

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
        if (container == null) return false;

        player = interactor.transform;

        CanvasSwitcher.Instance?.OpenContainer();
        return true;
    }
}
