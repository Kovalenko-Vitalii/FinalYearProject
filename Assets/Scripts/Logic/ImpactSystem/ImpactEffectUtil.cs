using UnityEngine;

public static class ImpactEffectUtil
{
    public static void SpawnImpact(
        RaycastHit hit,
        ImpactKind impactKind,
        ImpactEffectDatabase database)
    {
        if (database == null)
            return;

        SurfaceType surfaceType = ResolveSurfaceType(hit.collider);
        ImpactEffectEntry entry = database.GetEntry(surfaceType, impactKind);

        if (entry == null)
            return;

        PlayImpactParticle(hit, entry);
        PlayImpactSound(hit, entry);
    }

    private static void PlayImpactParticle(RaycastHit hit, ImpactEffectEntry entry)
    {
        if (entry.effectPrefab == null)
            return;

        Vector3 spawnPos = hit.point + hit.normal * 0.01f;
        Quaternion rotation = Quaternion.LookRotation(hit.normal);

        if (ParticleManager.Instance != null)
            ParticleManager.Instance.PlayOneShot(entry.effectPrefab, spawnPos, rotation);
    }

    private static void PlayImpactSound(RaycastHit hit, ImpactEffectEntry entry)
    {
        if (entry.soundClips == null || entry.soundClips.Length == 0)
            return;

        AudioClip clip = entry.soundClips[Random.Range(0, entry.soundClips.Length)];
        if (clip == null)
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayWorldOneShot(clip, hit.point, entry.soundVolume);
    }

    private static SurfaceType ResolveSurfaceType(Collider col)
    {
        if (col == null)
            return SurfaceType.Default;

        SurfaceTypeHolder holder = col.GetComponentInParent<SurfaceTypeHolder>();
        return holder != null ? holder.type : SurfaceType.Default;
    }
}