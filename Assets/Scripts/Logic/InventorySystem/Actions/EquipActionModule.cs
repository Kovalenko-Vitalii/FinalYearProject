using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Actions/Equip")]
public class EquipActionModule : ActionModule
{
    public override IEnumerable<ItemAction> GetActions(ItemActionContext ctx)
    {
        if (ctx.item == null || ctx.item.data is not GearData gearData)
            yield break;

        var im = InventoryManager.Instance;
        if (im == null)
            yield break;

        EquipmentSlotId slot = gearData.Slot;
        var equippedItem = im.GetEquippedItem(slot);
        bool isEquipped = ReferenceEquals(equippedItem, ctx.item);

        if (!isEquipped)
        {
            yield return new ItemAction
            {
                id = "equip",
                label = "Equip",
                slot = ActionSlot.Use,
                interactable = ctx.item.amount > 0,
                holdStartSound = gearData.onEquipSound,
                holdStartSoundId = UISoundId.EquipItem,
                execute = () =>
                {
                    im.TryEquipItem(ctx.source, ctx.item, slot);
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
                holdStartSound = gearData.onUnequipSound,
                holdStartSoundId = UISoundId.UnequipItem,
                execute = () =>
                {
                    im.TryUnequipItem(slot, ctx.source);
                }
            };
        }
    }
}