using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ItemData;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private bool isPlayerInventory = true;
    [SerializeField] private bool isStorageInventory = false;

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
    private Inventory targetInventory => GetTargetInventory();
    private void Awake()
    {
        buttonAll.onClick.AddListener(() => SetFilter(ItemTag.None, "All"));
        buttonComsumable.onClick.AddListener(() => SetFilter(ItemTag.Food, "Food"));
        buttonMedicine.onClick.AddListener(() => SetFilter(ItemTag.Medicine, "Medicine"));
        buttonGear.onClick.AddListener(() => SetFilter(ItemTag.Gear, "Cloth"));
        buttonMaterials.onClick.AddListener(() => SetFilter(ItemTag.Material, "Materials"));
        buttonTools.onClick.AddListener(() => SetFilter(ItemTag.Tool, "Tools"));
        buttonQuestItems.onClick.AddListener(() => SetFilter(ItemTag.Quest, "Quest Items"));
    }

    private void SetFilter(ItemTag tag, string label)
    {
        activeFilter = tag;
        if (selectedList != null) selectedList.text = label;
        Refresh();
    }

    private Inventory GetTargetInventory()
    {
        if (InventoryManager.Instance == null)
            return null;

        if (isPlayerInventory)
            return InventoryManager.Instance.playerInventory;

        if (isStorageInventory)
            return InventoryManager.Instance.storageInventory;

        return null;
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (targetInventory == null) return;

        foreach (Transform child in content)
            Destroy(child.gameObject);

        var source = targetInventory.items.AsEnumerable();

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
            obj.GetComponent<InventoryItemUI>().SetItem(item, targetInventory);
        }
    }
}
