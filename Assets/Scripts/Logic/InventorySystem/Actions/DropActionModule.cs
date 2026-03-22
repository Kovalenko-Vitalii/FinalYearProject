using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Actions/Drop")]
public class DropActionModule : ActionModule
{
    [SerializeField] private float dropImpulse = 3f;

    private static Transform cachedDropOrigin;
    private Transform GetDropOrigin()
    {
        if (cachedDropOrigin != null)
            return cachedDropOrigin;

        var go = GameObject.FindWithTag("PlayerDropOrigin");
        if (go != null)
            cachedDropOrigin = go.transform;

        return cachedDropOrigin;
    }
    public override IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        yield return new ItemAction
        {
            id = "drop",
            label = "Drop",
            slot = ActionSlot.Drop,
            interactable = ctx.item != null && ctx.item.amount > 0 && ctx.item.data != null && ctx.item.data.pickupPrefab != null,
            execute = () =>
            {
                if (ctx.item == null || ctx.item.data == null || ctx.source == null)
                    return;

                var origin = GetDropOrigin();
                Vector3 pos = origin != null ? origin.position : Vector3.zero;
                Vector3 impulse = origin != null ? origin.forward * dropImpulse : Vector3.zero;

                bool isInstanceItem = ctx.item.MustRemainUniqueInstance;

                if (isInstanceItem)
                {
                    InventoryItem loot = ctx.item.Clone();

                    if (!ctx.source.RemoveItemInstance(ctx.item))
                        return;

                    WorldObjectSpawner.Instance?.SpawnItem(loot, pos, Quaternion.identity, impulse);
                }
                else
                {
                    ctx.source.RemoveItem(ctx.item.data, 1);

                    InventoryItem loot = new InventoryItem(ctx.item.data, 1, ctx.item.currentDurability);
                    WorldObjectSpawner.Instance?.SpawnItem(loot, pos, Quaternion.identity, impulse);
                }

                SoundManager.Instance?.PlayUI(UISoundId.DropItem, ctx.item.data.onDropSound);
            }
        };
    }
}
