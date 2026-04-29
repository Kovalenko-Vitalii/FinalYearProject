using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private float lastClickTime = -10f;

    [Header("Double Click")]
    [SerializeField] private float doubleClickThreshold = 0.3f;

    private Inventory sourceInventory;
    private InventoryItem currentItem;
    private bool subscribed;

    public InventoryItem CurrentItem => currentItem;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

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
            button.onClick.AddListener(HandleClick);
        }

        Subscribe();
        RefreshSelectionState();
    }

    private void HandleClick()
    {
        if (currentItem == null)
            return;

        float now = Time.unscaledTime;
        bool isDoubleClick = now - lastClickTime <= doubleClickThreshold;
        lastClickTime = now;

        SelectCurrentItem(playClickSound: !isDoubleClick);

        if (isDoubleClick)
            ExecutePrimaryAction();
    }

    private void SelectCurrentItem(bool playClickSound = true)
    {
        var manager = InventoryManager.Instance;
        if (manager == null || currentItem == null)
            return;

        manager.SelectItem(currentItem, sourceInventory);

        var infoUI = Object.FindAnyObjectByType<ItemInfoUI>();
        if (infoUI != null)
            infoUI.SetItem(currentItem, sourceInventory);

        if (playClickSound)
        {
            SoundManager.Instance?.PlayUI(
                UISoundId.ItemClick,
                currentItem.data != null ? currentItem.data.onClickSound : null
            );
        }
    }

    private void ExecutePrimaryAction()
    {
        var manager = InventoryManager.Instance;
        if (manager == null || currentItem == null || sourceInventory == null)
            return;

        if (!ReferenceEquals(sourceInventory, manager.playerInventory))
            return;

        if (currentItem.data is not IItemActionProvider provider)
            return;

        var ctx = new ItemActionContext
        {
            source = sourceInventory,
            item = currentItem,
            equippedItems = manager.playerEquippedItems
        };

        var primary = provider.GetActions(ctx)
            .FirstOrDefault(a => a.slot == ActionSlot.Use && a.execute != null && a.interactable);

        if (primary.execute == null)
            return;

        SoundManager.Instance?.PlayUI(primary.holdStartSoundId, primary.holdStartSound);

        primary.execute.Invoke();

        var infoUI = Object.FindAnyObjectByType<ItemInfoUI>();

        if (sourceInventory != null && sourceInventory.items.Contains(currentItem))
        {
            manager.SelectItem(currentItem, sourceInventory);

            if (infoUI != null)
                infoUI.SetItem(currentItem, sourceInventory);
        }
        else
        {
            var sameDataItem = sourceInventory != null
                ? sourceInventory.items.FirstOrDefault(i => i != null && i.data == currentItem.data)
                : null;

            if (sameDataItem != null)
            {
                manager.SelectItem(sameDataItem, sourceInventory);

                if (infoUI != null)
                    infoUI.SetItem(sameDataItem, sourceInventory);
            }
            else
            {
                manager.ClearSelection();

                if (infoUI != null)
                    infoUI.ShowDefault();
            }
        }
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