using UnityEngine;
using UnityEngine.UI;

public class ContainerUI : MonoBehaviour
{
    [Header("Player / Container UI")]
    [SerializeField] private InventoryUI playerInventoryUI;
    [SerializeField] private InventoryUI containerInventoryUI;

    [Header("Actions")]
    [SerializeField] private Button buttonPut;
    [SerializeField] private Button buttonTake;

    private Inventory containerInventory;

    private WorldContainerInteractable sourceInteractable;


    private void Awake()
    {
        if (buttonPut != null)
            buttonPut.onClick.AddListener(OnPutClicked);

        if (buttonTake != null)
            buttonTake.onClick.AddListener(OnTakeClicked);
    }

    private void OnEnable()
    {
        var mgr = InventoryManager.Instance;
        if (mgr != null)
            mgr.OnSelectionChanged += HandleSelectionChanged;

        UpdateButtons();
    }

    private void OnDisable()
    {
        var mgr = InventoryManager.Instance;
        if (mgr != null)
            mgr.OnSelectionChanged -= HandleSelectionChanged;

        if (sourceInteractable != null)
        {
            sourceInteractable.CloseLid();
            sourceInteractable = null;
        }
    }

    public void ShowFor(WorldContainer worldContainer)
    {
        ShowFor(worldContainer, null);
    }

    public void ShowFor(WorldContainer worldContainer, WorldContainerInteractable source)
    {
        if (worldContainer == null) return;

        sourceInteractable = source;
        containerInventory = worldContainer.Inventory;

        if (playerInventoryUI != null)
        {
            playerInventoryUI.SetTargetInventory(null);
            playerInventoryUI.Refresh();
        }

        if (containerInventoryUI != null)
        {
            containerInventoryUI.SetTargetInventory(containerInventory);
            containerInventoryUI.Refresh();
        }

        InventoryManager.Instance?.ClearSelection();
        UpdateButtons();
    }

    public void Clear()
    {
        if (containerInventoryUI != null)
            containerInventoryUI.SetTargetInventory(null);

        containerInventory = null;
        InventoryManager.Instance?.ClearSelection();
        UpdateButtons();
    }

    private void HandleSelectionChanged(InventoryItem item, Inventory source)
    {
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool showPut = false;
        bool showTake = false;

        var mgr = InventoryManager.Instance;
        if (mgr != null && containerInventory != null)
        {
            var selected = mgr.SelectedItem;
            var source = mgr.SourceInventory;

            if (selected != null && source != null)
            {
                if (source == mgr.playerInventory)
                {
                    showPut = containerInventory.CanAdd(selected.data, selected.amount);
                }
                else if (source == containerInventory)
                {
                    showTake = mgr.playerInventory.CanAdd(selected.data, selected.amount);
                }
            }
        }

        if (buttonPut != null)
        {
            buttonPut.gameObject.SetActive(showPut);
            buttonPut.interactable = showPut;
        }

        if (buttonTake != null)
        {
            buttonTake.gameObject.SetActive(showTake);
            buttonTake.interactable = showTake;
        }
    }


    private void OnPutClicked()
    {
        var mgr = InventoryManager.Instance;
        if (mgr == null || containerInventory == null) return;

        var selected = mgr.SelectedItem;
        if (selected == null) return;

        mgr.MoveItem(mgr.playerInventory, containerInventory, selected.data, selected.amount);

        playerInventoryUI?.Refresh();
        containerInventoryUI?.Refresh();

        mgr.ClearSelection();
        UpdateButtons();
    }

    private void OnTakeClicked()
    {
        var mgr = InventoryManager.Instance;
        if (mgr == null || containerInventory == null) return;

        var selected = mgr.SelectedItem;
        if (selected == null) return;

        mgr.MoveItem(containerInventory, mgr.playerInventory, selected.data, selected.amount);

        playerInventoryUI?.Refresh();
        containerInventoryUI?.Refresh();

        mgr.ClearSelection();
        UpdateButtons();
    }
}
