using System.Collections.Generic;
using UnityEngine;

// This class represents a basic data object of the item
[CreateAssetMenu(menuName = "Items/ItemData")]
public class ItemData : ScriptableObject, IStatProvider, IItemActionProvider
{
    [System.Flags]
    public enum ItemTag
    {
        None = 0,
        Food = 1 << 0,
        Medicine = 1 << 2,
        Gear = 1 << 3,
        Material = 1 << 4,
        Tool = 1 << 5,
        Quest = 1 << 6,
    }

    [Header("Base")]
    public string id;
    public string itemName;
    public Sprite icon;
    public int maxStack;
    public float weight;
    [TextArea] public string description;

    public AudioClip onClickSound;
    public AudioClip onPickupSound;
    public AudioClip onDropSound;

    [Header("Durability")]
    public bool hasDurability = false;
    public float maxDurability = 0f;

    public float useDuration = 0f;

    [Header("Tags")]
    [SerializeField] private ItemTag tags = ItemTag.None;
    public ItemTag Tags => tags;
    public bool HasTag(ItemTag tag) => (tags & tag) != 0;

    [Header("Action Modules")]
    [SerializeField] private List<ActionModule> modules = new();

    public GameObject pickupPrefab;

#if UNITY_EDITOR
    protected void EnsureTag(ItemTag tag) => tags |= tag;
#endif

    public virtual IEnumerable<StatValue> GetStats()
    {
        if (weight != 0) yield return new StatValue { id = StatId.Weight, value = weight };
    }

    public virtual IEnumerable<StatValue> GetBaselineForCompare()
        => System.Array.Empty<StatValue>();

    public virtual IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        foreach (var m in modules)
            if (m != null)
                foreach (var a in m.GetActions(ctx))
                    yield return a;
    }
}
