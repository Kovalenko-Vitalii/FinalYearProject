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
}

public struct ItemActionContext
{
    public Inventory source;
    public InventoryItem item;
    public Equipment equipment;
}

public interface IItemActionProvider
{
    System.Collections.Generic.IEnumerable<ItemAction> GetActions(ItemActionContext ctx);
}