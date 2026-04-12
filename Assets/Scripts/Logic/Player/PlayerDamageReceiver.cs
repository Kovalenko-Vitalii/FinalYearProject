using UnityEngine;

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