using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Actions/Held Equip")]
public class HeldEquipActionModule : ActionModule
{
    public override IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        if (ctx.item == null || ctx.item.data is not HoldableItemData)
            yield break;

        var im = InventoryManager.Instance;
        if (im == null || ctx.source == null)
            yield break;

        bool isEquipped = im.playerEquippedItems != null &&
                          im.playerEquippedItems.Contains(ctx.item);

        yield return new ItemAction
        {
            id = isEquipped ? "unequip_held" : "equip_held",
            label = isEquipped ? "Unequip" : "Equip",
            slot = ActionSlot.Use,
            interactable = ctx.item.amount > 0,
            execute = () =>
            {
                im.TryToggleEquipItem(ctx.source, ctx.item);
            }
        };
    }
}