using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ItemData;

public class InventoryUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private bool usePlayerInventory = true;

    [Header("UI")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject itemPrefab;

    [Header("Filter Buttons")]
    [SerializeField] private Button buttonAll;
    [SerializeField] private Button buttonComsumable;
    [SerializeField] private Button buttonMedicine;
    [SerializeField] private Button buttonGear;
    [SerializeField] private Button buttonMaterials;
    [SerializeField] private Button buttonTools;
    [SerializeField] private Button buttonQuestItems;
    [SerializeField] private TextMeshProUGUI selectedList;

    private ItemTag activeFilter = ItemTag.None;
    private Inventory overrideInventory;
    private Inventory subscribedInventory;

    private Inventory TargetInventory
    {
        get
        {
            if (overrideInventory != null)
                return overrideInventory;

            if (usePlayerInventory)
                return InventoryManager.Instance != null ? InventoryManager.Instance.playerInventory : null;

            return null;
        }
    }

    public void SetTargetInventory(Inventory inventory)
    {
        overrideInventory = inventory;
        RebindAndRefresh();
    }

    public void ClearOverrideInventory()
    {
        overrideInventory = null;
        RebindAndRefresh();
    }

    private void Awake()
    {
        AddFilterListener(buttonAll, ItemTag.None, "All");
        AddFilterListener(buttonComsumable, ItemTag.Food, "Food");
        AddFilterListener(buttonMedicine, ItemTag.Medicine, "Medicine");
        AddFilterListener(buttonGear, ItemTag.Gear, "Cloth");
        AddFilterListener(buttonMaterials, ItemTag.Material, "Materials");
        AddFilterListener(buttonTools, ItemTag.Tool, "Tools");
        AddFilterListener(buttonQuestItems, ItemTag.Quest, "Quest Items");
    }

    private void OnEnable()
    {
        RebindAndRefresh();
    }

    private void Start()
    {
        RebindAndRefresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromInventory();
    }

    private void AddFilterListener(Button button, ItemTag tag, string label)
    {
        if (button == null)
            return;

        button.onClick.AddListener(() => SetFilter(tag, label));
    }

    private void SetFilter(ItemTag tag, string label)
    {
        activeFilter = tag;

        if (selectedList != null)
            selectedList.text = label;

        Refresh();
    }

    private void RebindAndRefresh()
    {
        SubscribeToInventory();
        Refresh();
    }

    private void SubscribeToInventory()
    {
        Inventory target = TargetInventory;

        if (ReferenceEquals(subscribedInventory, target))
            return;

        UnsubscribeFromInventory();

        subscribedInventory = target;

        if (subscribedInventory != null)
            subscribedInventory.OnChanged += HandleInventoryChanged;
    }

    private void UnsubscribeFromInventory()
    {
        if (subscribedInventory != null)
            subscribedInventory.OnChanged -= HandleInventoryChanged;

        subscribedInventory = null;
    }

    private void HandleInventoryChanged()
    {
        Refresh();
    }

    public void Refresh()
    {
        SubscribeToInventory();

        Inventory inventory = TargetInventory;

        if (content == null || itemPrefab == null)
            return;

        foreach (Transform child in content)
            Destroy(child.gameObject);

        if (inventory == null)
            return;

        var source = inventory.items.AsEnumerable();

        if (activeFilter != ItemTag.None)
        {
            source = source.Where(i =>
            {
                ItemData data = i?.data;
                return data != null && data.HasTag(activeFilter);
            });
        }

        foreach (InventoryItem item in source)
        {
            if (item == null || item.data == null)
                continue;

            GameObject obj = Instantiate(itemPrefab, content);
            InventoryItemUI ui = obj.GetComponent<InventoryItemUI>();
            if (ui != null)
                ui.SetItem(item, inventory);
        }
    }
}