using UnityEngine;

// This struct represents unified damage action data in the game
public struct DamageData
{
    public int amount;
    public Vector3 hitPoint;
    public Vector3 hitDirection;
    public GameObject source;
    public DamageType damageType;
}

public enum DamageType
{
    Generic,
    Axe,
    Pickaxe,
    Bullet
}

// This interface is applied to AI creatures that can take damage
public interface IDamageable
{
    void TakeDamage(DamageData damage);
}