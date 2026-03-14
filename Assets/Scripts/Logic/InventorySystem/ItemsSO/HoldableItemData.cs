using UnityEngine;

[CreateAssetMenu(menuName = "Items/HoldableData")]
public class HoldableItemData : ItemData
{
    public HeldItemKind heldItemKind;
    public GameObject firstPersonPrefab;

    public bool supportsPrimary = true;
    public bool supportsSecondary = false;
    public bool supportsReload = false;
    public bool supportsAim = false;
}

public enum HeldItemKind
{
    None,
    Pistol,
    Axe,
    Pickaxe
}