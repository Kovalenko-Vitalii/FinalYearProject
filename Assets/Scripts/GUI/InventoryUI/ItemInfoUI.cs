using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoUI : MonoBehaviour
{
    [Header("Header UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private Button buttonEquip;
    [SerializeField] private Button buttonDelete;
    [SerializeField] private Button buttonActions;

    [Header("Stats UI")]
    [SerializeField] private StatPanelRenderer statPanel;

    [Header("Defaults")]
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private string defaultName = "";
    [SerializeField, TextArea] private string defaultDescription = "";

    private Inventory trackedSource;
    private Inventory subscribedSource;
    private InventoryItem trackedItem;

    private bool subscribedToManager;

    private void Awake()
    {
        ShowDefault();
    }

    private void OnDisable()
    {
        UnsubscribeFromSource();
        UnsubscribeFromManager();
    }

    public void SetItem(InventoryItem invItem, Inventory source)
    {
        trackedItem = invItem;
        trackedSource = source;

        SubscribeToManager();
        SubscribeToSource();

        RefreshTrackedItem();
    }

    public void ShowDefault()
    {
        if (icon != null)
            icon.sprite = defaultIcon;

        if (itemName != null)
            itemName.text = defaultName;

        if (itemDescription != null)
            itemDescription.text = defaultDescription;

        if (buttonEquip != null)
        {
            buttonEquip.onClick.RemoveAllListeners();
            buttonEquip.gameObject.SetActive(false);
        }

        if (buttonDelete != null)
        {
            buttonDelete.onClick.RemoveAllListeners();
            buttonDelete.gameObject.SetActive(false);
        }

        if (buttonActions != null)
        {
            buttonActions.onClick.RemoveAllListeners();
            buttonActions.gameObject.SetActive(false);
        }

        if (statPanel != null)
            statPanel.Clear();

        trackedItem = null;
        trackedSource = null;

        UnsubscribeFromSource();
    }

    private void SubscribeToManager()
    {
        if (subscribedToManager)
            return;

        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnEquipmentChanged += OnEquipmentChanged;
            subscribedToManager = true;
        }
    }

    private void UnsubscribeFromManager()
    {
        if (!subscribedToManager)
            return;

        var im = InventoryManager.Instance;
        if (im != null)
            im.OnEquipmentChanged -= OnEquipmentChanged;

        subscribedToManager = false;
    }

    private void SubscribeToSource()
    {
        if (ReferenceEquals(subscribedSource, trackedSource))
            return;

        UnsubscribeFromSource();

        subscribedSource = trackedSource;
        if (subscribedSource != null)
            subscribedSource.OnChanged += OnSourceChanged;
    }

    private void UnsubscribeFromSource()
    {
        if (subscribedSource != null)
            subscribedSource.OnChanged -= OnSourceChanged;

        subscribedSource = null;
    }

    private void OnSourceChanged()
    {
        RefreshTrackedItem();
    }

    private void OnEquipmentChanged()
    {
        RefreshTrackedItem();
    }

    private void RefreshTrackedItem()
    {
        if (trackedSource == null || trackedItem == null)
        {
            ShowDefault();
            return;
        }

        if (!ContainsTrackedItem())
        {
            ShowDefault();
            return;
        }

        var data = trackedItem.data;
        if (data == null)
        {
            ShowDefault();
            return;
        }

        if (icon != null)
            icon.sprite = data.icon != null ? data.icon : defaultIcon;

        if (itemName != null)
            itemName.text = string.IsNullOrEmpty(data.itemName) ? defaultName : data.itemName;

        if (itemDescription != null)
            itemDescription.text = string.IsNullOrEmpty(data.description) ? defaultDescription : data.description;

        if (statPanel != null)
            statPanel.Render(trackedItem);

        ActionBinder.BindFixedButtons(
            dropButton: buttonDelete,
            primaryButton: buttonEquip,
            actionsButton: buttonActions,
            invItem: trackedItem,
            source: trackedSource,
            afterActionRefresh: RefreshTrackedItem,
            primaryFallbackLabel: "Use"
        );
    }

    private bool ContainsTrackedItem()
    {
        if (trackedSource == null || trackedItem == null)
            return false;

        return trackedSource.items.Contains(trackedItem);
    }
}