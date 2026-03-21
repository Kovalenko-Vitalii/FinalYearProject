using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Actions/Held Equip")]
public class HeldEquipActionModule : ActionModule
{
    public override IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        if (ctx.item.data is not HoldableItemData)
            yield break;

        var im = InventoryManager.Instance;
        if (im == null)
            yield break;
        bool isEquipped = im.playerHeldEquipment.Contains(ctx.item);

        yield return new ItemAction
        {
            id = isEquipped ? "unequip_held" : "equip_held",
            label = isEquipped ? "Unequip" : "Equip",
            slot = ActionSlot.Use,
            interactable = ctx.item.amount > 0,
            execute = () =>
            {
                im.TryToggleEquipHeldItem(ctx.item);

                var inventoryUi = FindAnyObjectByType<InventoryUI>();
                if (inventoryUi != null) inventoryUi.Refresh();

                var gearUI = Object.FindAnyObjectByType<GearUI>(); if (gearUI) gearUI.Refresh();
                foreach (var invUI in Object.FindObjectsByType<InventoryUI>(FindObjectsSortMode.None)) invUI.Refresh();
            }
        };
    }
}