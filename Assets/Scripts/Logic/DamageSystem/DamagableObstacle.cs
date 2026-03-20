using UnityEngine;

// This script represents a damageble obstacle that can receive selected type of damage.
public class DamageableObstacle : MonoBehaviour, IDamageable
{
    [SerializeField] int hp = 30;
    [SerializeField] bool acceptsAxe = true;
    [SerializeField] bool acceptsPickaxe = false;
    [SerializeField] bool acceptsBullet = false;

    public void TakeDamage(DamageData damage)
    {
        if (!CanBeDamagedBy(damage.damageType))
            return;

        hp -= damage.amount;
        GameLog.Log("OBSTACLE", $"{name} took {damage.amount} from {damage.damageType}. HP = {hp}");

        if (hp <= 0)
            Break();
    }

    // This is a temporary solution !
    private bool CanBeDamagedBy(DamageType type)
    {
        return type switch
        {
            DamageType.Axe => acceptsAxe,
            DamageType.Pickaxe => acceptsPickaxe,
            DamageType.Bullet => acceptsBullet,
            DamageType.Generic => false,
            _ => false
        };
    }

    private void Break()
    {
        Destroy(gameObject);
    }
}