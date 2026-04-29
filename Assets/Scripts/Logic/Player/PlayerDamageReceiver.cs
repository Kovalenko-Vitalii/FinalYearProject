using UnityEngine;

// This simple class is respobsible for applying damage to player if received
public class PlayerDamageReceiver : MonoBehaviour
{
    public void ReceiveHit(PlayerHitData hit)
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null || stats.IsDead)
            return;

        stats.ApplyIncomingDamage(hit.Damage);
    }
}