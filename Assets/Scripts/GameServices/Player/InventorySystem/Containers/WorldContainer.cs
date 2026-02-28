using System;
using UnityEngine;

public class WorldContainer : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] private string id;
    public string Id => id;

    [Header("Inventory")]
    [SerializeField] private int slotLimit = -1;
    [SerializeField] private string[] allowedTags;

    public Inventory Inventory { get; private set; }

    private void Reset()
    {
#if UNITY_EDITOR
        SaveIdUtil.EnsureId(ref id, this);
#else
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SaveIdUtil.EnsureId(ref id, this);
    }
#endif

    private void Awake()
    {
        Inventory = new Inventory(new StorageInventoryPolicy(slotLimit, allowedTags));
    }
}