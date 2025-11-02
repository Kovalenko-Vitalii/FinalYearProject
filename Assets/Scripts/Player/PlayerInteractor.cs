using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float useDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private FPSUIManager ui;

    private IInteractable current;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    private void Update()
    {
        UpdateHover();

        if (Input.GetKeyDown(interactKey) && current != null)
        {
            if (current.Interact(this))
            {
                //something happens (sound/anim)
            }
        }
    }

    private void UpdateHover()
    {
        string label = "";

        current = RaycastForInteractable();
        if (current != null && current.TryGetPrompt(this, out var prompt) && !string.IsNullOrEmpty(prompt))
            label = prompt;

        if (ui) ui.SetUnderCrosshairLabel(label);
    }

    private IInteractable RaycastForInteractable()
    {
        if (!cam) return null;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out var hit, useDistance, interactMask, QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<IInteractable>();

        return null;
    }

    public Inventory PlayerInventory => InventoryManager.Instance.playerInventory;
    public Transform DropOrigin => InventoryManager.Instance.playerDropOrigin;
}
