using UnityEngine;

[CreateAssetMenu(menuName = "InteractActions/Disable Interactable")]
public class DisableInteractableAction : InteractAction
{
    public override void Execute(InteractContext ctx)
    {
        if (ctx?.interactable == null) return;
        ctx.interactable.ApplyStateImmediate(false);
    }
}