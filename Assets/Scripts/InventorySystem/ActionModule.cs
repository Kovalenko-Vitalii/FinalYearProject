using System.Collections.Generic;
using UnityEngine;

public abstract class ActionModule : ScriptableObject, IItemActionProvider
{
    public abstract IEnumerable<ItemAction> GetActions(ItemActionContext ctx);
}
