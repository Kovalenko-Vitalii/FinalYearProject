using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Actions/Use Consumable")]
public class UseConsumableActionModule : ActionModule
{
    public enum UseVerb { Use, Eat, Drink }
    public UseVerb verb = UseVerb.Use;

    public override IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        if (ctx.item.data is not ConsumableData cd) yield break;

        string label = verb switch { UseVerb.Eat => "Eat", UseVerb.Drink => "Drink", _ => "Use" };

        yield return new ItemAction
        {
            id = "use",
            label = label,
            slot = ActionSlot.Use,
            interactable = ctx.item.amount > 0,
            execute = () =>
            {
                ctx.source.RemoveItem(cd, 1);
                var ui = Object.FindAnyObjectByType<InventoryUI>(); if (ui) ui.Refresh();
            }
        };
    }
}
