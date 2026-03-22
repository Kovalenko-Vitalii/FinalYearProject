using System;
using System.Collections.Generic;
using UnityEngine;

public enum ActionSlot { Use, Secondary, Drop }

public struct ItemAction
{
    public string id;
    public string label;
    public bool interactable;
    public Action execute;

    public ActionSlot slot;

    public AudioClip holdStartSound;
    public UISoundId holdStartSoundId;
}

public struct ItemActionContext
{
    public Inventory source;
    public InventoryItem item;
    public EquippedItems equippedItems;
}

public interface IItemActionProvider
{
    IEnumerable<ItemAction> GetActions(ItemActionContext ctx);
}