using UnityEngine;

public abstract class InteractAction : ScriptableObject
{
    public abstract void Execute(InteractContext ctx);
}