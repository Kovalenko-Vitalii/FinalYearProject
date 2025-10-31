using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Actions/Drop")]
public class DropActionModule : ActionModule
{
    public override IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        yield return new ItemAction
        {
            id = "drop",
            label = "Drop",
            slot = ActionSlot.Drop,
            interactable = ctx.item.amount > 0,
            execute = () =>
            {
                ctx.source.RemoveItem(ctx.item.data, 1);

                var ui = Object.FindAnyObjectByType<InventoryUI>();
                if (ui != null) ui.Refresh();

                var gearUI = Object.FindAnyObjectByType<GearUI>();
                if (gearUI != null) gearUI.Refresh();
            }
        };
    }
}
