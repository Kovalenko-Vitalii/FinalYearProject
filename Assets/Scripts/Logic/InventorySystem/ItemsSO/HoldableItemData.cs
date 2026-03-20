using UnityEngine;

public class HoldableItemData : ItemData
{
    public GameObject firstPersonPrefab;

    public bool supportsPrimary = true;
    public bool supportsSecondary = false;
    public bool supportsReload = false;
    public bool supportsAim = false;
}
