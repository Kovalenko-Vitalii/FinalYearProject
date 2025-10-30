using UnityEngine;

public class ItemData : ScriptableObject {

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
    

    [Header("Tags")]
    [SerializeField] private ItemTag tags = ItemTag.None;
    public ItemTag Tags => tags;

    public bool HasTag(ItemTag tag) => (tags & tag) != 0;

#if UNITY_EDITOR
    protected void EnsureTag(ItemTag tag) => tags |= tag;
#endif
}

