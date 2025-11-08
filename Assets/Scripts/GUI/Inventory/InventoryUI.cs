using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ItemData;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory targetInventory;
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

    private Inventory TargetInventory
    {
        get
        {
            if (overrideInventory != null)
                return overrideInventory;

            if (InventoryManager.Instance == null)
                return null;

            return InventoryManager.Instance.playerInventory;
        }
    }

    public void SetTargetInventory(Inventory inventory)
    {
        overrideInventory = inventory;
        if (isActiveAndEnabled)
            Refresh();
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

    private void AddFilterListener(Button button, ItemTag tag, string label)
    {
        if (button == null) return;
        button.onClick.AddListener(() => SetFilter(tag, label));
    }

    private void SetFilter(ItemTag tag, string label)
    {
        activeFilter = tag;
        if (selectedList != null) selectedList.text = label;
        Refresh();
    }


    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var inventory = TargetInventory;
        if (inventory == null) return;

        foreach (Transform child in content)
            Destroy(child.gameObject);

        var source = inventory.items.AsEnumerable();

        if (activeFilter != ItemTag.None)
        {
            source = source.Where(i =>
            {
                var data = i.data;
                return data != null && data.HasTag(activeFilter);
            });
        }

        foreach (var item in source)
        {
            var obj = Instantiate(itemPrefab, content);
            obj.GetComponent<InventoryItemUI>().SetItem(item, inventory);
        }
    }

}
