using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private Button button;

    [Header("Highlight")]
    [SerializeField] private GameObject selectionHighlight;

    private Inventory sourceInventory;
    private InventoryItem currentItem;
    private bool subscribed;

    public InventoryItem CurrentItem => currentItem;

    private void OnEnable()
    {
        Subscribe();
        RefreshSelectionState();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void SetItem(InventoryItem item, Inventory source)
    {
        currentItem = item;
        sourceInventory = source;

        if (icon != null)
            icon.sprite = item != null && item.data != null ? item.data.icon : null;

        if (itemName != null)
            itemName.text = item != null && item.data != null ? item.data.itemName : "";

        if (weightText != null)
            weightText.text = item != null && item.data != null
                ? (item.data.weight * item.amount).ToString("0.##") + " kg"
                : "";

        if (amountText != null)
            amountText.text = item != null && item.amount > 1 ? item.amount.ToString() : "";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnItemClicked);
        }

        Subscribe();
        RefreshSelectionState();
    }

    private void OnItemClicked()
    {
        var manager = InventoryManager.Instance;
        if (manager == null || currentItem == null)
            return;

        manager.SelectItem(currentItem, sourceInventory);

        var infoUI = Object.FindAnyObjectByType<ItemInfoUI>();
        if (infoUI != null)
            infoUI.SetItem(currentItem, sourceInventory);

        SoundManager.Instance?.PlayUI(
            UISoundId.ItemClick,
            currentItem.data != null ? currentItem.data.onClickSound : null
        );
    }

    private void Subscribe()
    {
        if (subscribed)
            return;

        var manager = InventoryManager.Instance;
        if (manager != null)
        {
            manager.OnSelectionChanged += HandleSelectionChanged;
            subscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        var manager = InventoryManager.Instance;
        if (manager != null)
            manager.OnSelectionChanged -= HandleSelectionChanged;

        subscribed = false;
    }

    private void HandleSelectionChanged(InventoryItem item, Inventory source)
    {
        bool isSelected =
            currentItem != null &&
            ReferenceEquals(item, currentItem) &&
            ReferenceEquals(source, sourceInventory);

        SetHighlight(isSelected);
    }

    private void RefreshSelectionState()
    {
        var manager = InventoryManager.Instance;

        bool isSelected =
            manager != null &&
            currentItem != null &&
            ReferenceEquals(manager.SelectedItem, currentItem) &&
            ReferenceEquals(manager.SourceInventory, sourceInventory);

        SetHighlight(isSelected);
    }

    public void SetHighlight(bool active)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(active);
    }
}