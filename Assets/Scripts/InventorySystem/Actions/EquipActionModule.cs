using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Actions/Equip")]
public class EquipActionModule : ActionModule
{
    public override IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        if (ctx.item.data is not GearData g) yield break;

        var im = InventoryManager.Instance;
        var eq = im.playerEquipment.GetEquipped(g.slot);
        bool isEquipped = ReferenceEquals(eq, g);
        float currentDurability = ctx.item.currentDurability;

        if (!isEquipped)
        {
            yield return new ItemAction
            {
                id = "equip",
                label = "Equip",
                slot = ActionSlot.Use,
                interactable = ctx.item.amount > 0,
                execute = () =>
                {
                    var old = im.playerEquipment.Equip(g);
                    ctx.source.RemoveItem(g, 1);
                    if (old != null) ctx.source.AddItem(old, 1, currentDurability);
                    var gearUI = Object.FindAnyObjectByType<GearUI>(); if (gearUI) gearUI.Refresh();
                    foreach (var invUI in Object.FindObjectsByType<InventoryUI>(FindObjectsSortMode.None)) invUI.Refresh();
                }
            };
        }
        else
        {
            yield return new ItemAction
            {
                id = "unequip",
                label = "Unequip",
                slot = ActionSlot.Use,
                interactable = true,
                execute = () =>
                {
                    im.playerEquipment.Unequip(g.slot);
                    ctx.source.AddItem(g, 1, currentDurability);
                    var gearUI = Object.FindAnyObjectByType<GearUI>(); if (gearUI) gearUI.Refresh();
                    foreach (var invUI in Object.FindObjectsByType<InventoryUI>(FindObjectsSortMode.None)) invUI.Refresh();
                }
            };
        }
    }
}
