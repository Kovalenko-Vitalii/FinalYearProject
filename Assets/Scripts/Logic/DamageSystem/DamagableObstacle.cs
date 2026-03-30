using System;
using UnityEngine;

// This script represents a damageble obstacle that can receive selected type of damage.
public class DamageableObstacle : MonoBehaviour, IDamageable, ISaveable
{
    [Header("Save")]
    [SerializeField] private string id;

    [SerializeField] private bool active;

    [SerializeField] int hp = 30;
    [SerializeField] bool acceptsAxe = true;
    [SerializeField] bool acceptsPickaxe = false;
    [SerializeField] bool acceptsBullet = false;

    public string SaveId => id;

    private void Reset()
    {
    #if UNITY_EDITOR
        SaveIdUtil.EnsureId(ref id, this);
    #else
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
    #endif
    }

    #if UNITY_EDITOR
    private void OnValidate() => SaveIdUtil.EnsureId(ref id, this);
#endif

    public object CaptureState()
    {
        return new DamagableObstacleWorldState { active = active, hp = hp };
    }

    public void RestoreState(object state)
    {
        if (state is not DamagableObstacleWorldState s) return;
        hp = s.hp;
        if (!active) Break();
    }

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
        gameObject.SetActive(false);
    }
}

[Serializable]
public struct DamagableObstacleWorldState
{
    public bool active;
    public int hp;
}