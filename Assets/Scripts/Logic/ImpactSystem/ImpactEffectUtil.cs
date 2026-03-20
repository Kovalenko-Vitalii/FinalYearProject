using UnityEngine;

public static class ImpactEffectUtil
{
    public static void SpawnImpact(
        RaycastHit hit,
        Collider hitCollider,
        ImpactKind impactKind,
        ImpactEffectDatabase database)
    {
        Debug.Log("0000000");
        if (database == null)
            return;
        Debug.Log("1111111");
        SurfaceType surfaceType = ResolveSurfaceType(hitCollider);
        ImpactEffectEntry entry = database.GetEntry(surfaceType, impactKind);

        if (entry == null || entry.effectPrefab == null)
            return;

        Debug.Log("222222");
        Vector3 spawnPos = hit.point + hit.normal * 0.01f;
        Quaternion rotation = Quaternion.LookRotation(hit.normal);

        GameObject fx = Object.Instantiate(entry.effectPrefab, spawnPos, rotation);

        float lifetime = entry.lifetime > 0f ? entry.lifetime : 2f;
        Object.Destroy(fx, lifetime);
    }

    public static SurfaceType ResolveSurfaceType(Collider col)
    {
        if (col == null)
            return SurfaceType.Default;

        SurfaceTypeHolder holder = col.GetComponentInParent<SurfaceTypeHolder>();
        if (holder != null)
            return holder.type;

        return SurfaceType.Default;
    }
}