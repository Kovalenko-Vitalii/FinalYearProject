using System.Linq;
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
    private ItemData trackedItem;
    private int trackedCount;
    private bool subscribed;

    void Awake() => ShowDefault();

    public void SetItem(InventoryItem invItem, Inventory source)
    {
        if (invItem == null || invItem.data == null)
        {
            ShowDefault();
            return;
        }

        trackedSource = source;
        trackedItem = invItem.data;
        trackedCount = InventoryUtil.Count(trackedSource, trackedItem);

        var data = invItem.data;
        if (icon) icon.sprite = data.icon ? data.icon : defaultIcon;
        if (itemName) itemName.text = string.IsNullOrEmpty(data.itemName) ? defaultName : data.itemName;
        if (itemDescription) itemDescription.text = string.IsNullOrEmpty(data.description) ? defaultDescription : data.description;

        if (statPanel != null) statPanel.Render(data);

        if (buttonEquip) buttonEquip.gameObject.SetActive(true);
        if (buttonDelete) buttonDelete.gameObject.SetActive(true);

        if (buttonActions) buttonActions.gameObject.SetActive(false);

        ActionBinder.BindFixedButtons(
            dropButton: buttonDelete,
            primaryButton: buttonEquip,
            actionsButton: buttonActions,
            invItem: InventoryUtil.MakeItem(source, data),
            source: source,
            afterActionRefresh: AfterActionRefresh,
            primaryFallbackLabel: "Use"
        );
     
        Subscribe();
    }

    public void ShowDefault()
    {
        Debug.Log("22");
        if (icon) icon.sprite = defaultIcon;
        if (itemName) itemName.text = defaultName;
        if (itemDescription) itemDescription.text = defaultDescription;

        if (buttonEquip) buttonEquip.gameObject.SetActive(false);
        if (buttonDelete) buttonDelete.gameObject.SetActive(false);
        if (buttonActions) buttonActions.gameObject.SetActive(false);

        trackedSource = null;
        trackedItem = null;
        trackedCount = 0;

        statPanel.Clear();
    }

    private void Subscribe()
    {
        if (subscribed) return;
        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnPlayerInventoryChanged += OnInventoryChanged;
            if (im.playerEquipment != null)
                im.playerEquipment.OnChanged += OnEquipmentChanged;
            subscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnPlayerInventoryChanged -= OnInventoryChanged;
            if (im.playerEquipment != null)
                im.playerEquipment.OnChanged -= OnEquipmentChanged;
        }
        subscribed = false;
    }

    private void OnDestroy() => Unsubscribe();

    private void OnInventoryChanged()
    {
        if (trackedSource == null || trackedItem == null) return;

        int current = InventoryUtil.Count(trackedSource, trackedItem);
        if (current == 0)
        {
            ShowDefault();
            return;
        }

        if (current != trackedCount)
        {
            trackedCount = current;
            ActionBinder.BindFixedButtons(
                buttonDelete,
                buttonEquip,
                buttonActions,
                InventoryUtil.MakeItem(trackedSource, trackedItem),
                trackedSource,
                AfterActionRefresh,
                "Use"
            );
        }
    }

    private void OnEquipmentChanged(GearData.GearSlot slot, GearData oldGear, GearData newGear)
    {
        if (trackedItem is not GearData g) return;
        if (g.slot != slot) return;

        if (ReferenceEquals(newGear, trackedItem) && InventoryUtil.Count(trackedSource, trackedItem) == 0)
        {
            ShowDefault();
            return;
        }

        trackedCount = InventoryUtil.Count(trackedSource, trackedItem);

        ActionBinder.BindFixedButtons(
            buttonDelete,
            buttonEquip,
            buttonActions,
            InventoryUtil.MakeItem(trackedSource, trackedItem),
            trackedSource,
            AfterActionRefresh,
            "Use"
        );
    }

    private void AfterActionRefresh()
    {
        if (trackedSource == null || trackedItem == null) { ShowDefault(); return; }

        trackedCount = InventoryUtil.Count(trackedSource, trackedItem);
        if (trackedCount == 0) { ShowDefault(); return; }

        ActionBinder.BindFixedButtons(
            buttonDelete,
            buttonEquip,
            buttonActions,
            InventoryUtil.MakeItem(trackedSource, trackedItem),
            trackedSource,
            AfterActionRefresh,
            "Use"
        );
    }
}
