using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Actions/Held Equip")]
public class HeldEquipActionModule : ActionModule
{
    public override IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        if (ctx.item.data is not HoldableItemData holdable)
            yield break;

        var im = InventoryManager.Instance;
        bool isAssigned = im.playerHeldEquipment.Contains(holdable);

        yield return new ItemAction
        {
            id = isAssigned ? "unequip_held" : "equip_held",
            label = isAssigned ? "Unequip" : "Equip",
            slot = ActionSlot.Use,
            interactable = ctx.item.amount > 0,
            execute = () =>
            {
                im.TryToggleEquipHeldItem(ctx.item);

                var inventoryUi = Object.FindAnyObjectByType<InventoryUI>();
                if (inventoryUi != null) inventoryUi.Refresh();
            }
        };
    }
}