using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    public HoldableItemData Data { get; private set; }
    protected PlayerItemController Owner { get; private set; }

    public virtual void Initialize(PlayerItemController owner, HoldableItemData data)
    {
        Owner = owner;
        Data = data;
    }

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }

    public virtual void OnPrimaryPressed() { }
    public virtual void OnPrimaryReleased() { }

    public virtual void OnSecondaryPressed() { }
    public virtual void OnSecondaryReleased() { }

    public virtual void OnReloadPressed() { }

    public virtual void OnSprintStarted() { }
    public virtual void OnSprintStopped() { }
}