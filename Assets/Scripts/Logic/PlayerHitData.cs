using UnityEngine;

public readonly struct PlayerHitData
{
    public readonly float Damage;
    public readonly GameObject Source;
    public readonly Vector3 HitPoint;

    public PlayerHitData(float damage, GameObject source, Vector3 hitPoint)
    {
        Damage = damage;
        Source = source;
        HitPoint = hitPoint;
    }
}