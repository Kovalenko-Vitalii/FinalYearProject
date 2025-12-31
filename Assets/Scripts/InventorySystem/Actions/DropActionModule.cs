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
            interactable = ctx.item.amount > 0 && ctx.item.data != null && ctx.item.data.pickupPrefab != null,
            execute = () =>
            {
                ctx.source.RemoveItem(ctx.item.data, 1);

                var origin = GetDropOrigin();
                Vector3 pos = origin ? origin.position : Vector3.zero;
                Vector3 impulse = origin ? origin.forward * dropImpulse : Vector3.zero;

                WorldObjectSpawner.Instance.SpawnItem(ctx.item.data, 1, ctx.item.currentDurability, pos, Quaternion.identity, impulse);

                var ui = Object.FindAnyObjectByType<InventoryUI>();
                if (ui != null) ui.Refresh();

                var gearUI = Object.FindAnyObjectByType<GearUI>();
                if (gearUI != null) gearUI.Refresh();
            }
        };
    }
}
