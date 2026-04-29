using UnityEngine;

// This class represents action that disables interactible object

[CreateAssetMenu(menuName = "InteractActions/Disable Executor")]
public class DisableExecutorAction : InteractAction
{
    public override void Execute(InteractContext ctx)
    {
        if (ctx?.executor == null) return;
        ctx.executor.ApplyStateImmediate(false);
    }
}