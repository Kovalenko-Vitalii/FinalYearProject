using System.Collections.Generic;
using UnityEngine;

// This class represents a consume action for consumable item
[CreateAssetMenu(menuName = "Items/Actions/Use Consumable")]
public class UseConsumableActionModule : ActionModule
{
    public enum UseVerb { Use, Eat, Drink }
    public UseVerb verb = UseVerb.Use;

    public float durabilityCost = 1f;

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
            holdStartSound = cd.onConsumeSound,
            holdStartSoundId = UISoundId.ConsumeItem,
            execute = () =>
            {
                var stats = PlayerStatManager.Instance;
                if (stats != null)
                {
                    stats.ApplyConsumable(cd);
                    ApplyStatusOps(cd);
                }
                else
                {
                    Debug.LogWarning("PlayerStatManager.Instance == null");
                }

                var invItem = ctx.item;

                if (invItem.HasDurability && durabilityCost > 0f)
                {
                    bool broken = invItem.Damage(durabilityCost);
                    if (broken)
                    {
                        ctx.source.RemoveFromStack(invItem, 1);
                    }
                }
                else
                {
                    ctx.source.RemoveItem(cd, 1);
                }

                var ui = Object.FindAnyObjectByType<InventoryUI>();
                if (ui) ui.Refresh();

                GameEvents.RaiseConsumed(ctx.item.data.id, ctx.item.data.Tags, 1);
            }
        };
    }

    private void ApplyStatusOps(ConsumableData cd)
    {
        if (cd.statusOps == null || cd.statusOps.Count == 0)
            return;

        var mgr = StatusEffectManager.Instance;
        if (mgr == null)
            return;

        foreach (var op in cd.statusOps)
        {
            switch (op.opType)
            {
                case ConsumableStatusOp.OpType.AddEffect:
                    AddEffect(op, mgr);
                    break;

                case ConsumableStatusOp.OpType.RemoveEffect:
                    RemoveEffect(op, mgr);
                    break;
                case ConsumableStatusOp.OpType.None:
                    break;
            }
        }
    }

    private void AddEffect(ConsumableStatusOp op, StatusEffectManager mgr)
    {
        BodyPart? part = op.affectAllParts ? null : op.targetPart;

        var effect = StatusEffectFactory.Create(op.effectId, op.duration, op.magnitude, part);
        if (effect != null)
            mgr.AddEffect(effect);
    }

    private void RemoveEffect(ConsumableStatusOp op, StatusEffectManager mgr)
    {
        if (op.affectAllParts)
            mgr.RemoveEffect(op.effectId, null);
        else
            mgr.RemoveEffect(op.effectId, op.targetPart);
    }
}
