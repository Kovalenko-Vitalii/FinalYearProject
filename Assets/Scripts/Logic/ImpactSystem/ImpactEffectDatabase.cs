using System;
using UnityEngine;

// This scriptable object stores VFX interaction data
[CreateAssetMenu(menuName = "VFX/Impact Database")]
public class ImpactEffectDatabase : ScriptableObject
{
    [SerializeField] private ImpactEffectEntry[] entries;

    public ImpactEffectEntry GetEntry(SurfaceType surfaceType, ImpactKind impactKind)
    {
        if (entries == null)
            return null;

        ImpactEffectEntry fallbackBySurface = null;
        ImpactEffectEntry fallbackDefault = null;

        foreach (var entry in entries)
        {
            if (entry == null)
                continue;

            if (entry.surfaceType == surfaceType && entry.impactKind == impactKind)
                return entry;

            if (entry.surfaceType == surfaceType && entry.impactKind == ImpactKind.Default)
                fallbackBySurface = entry;

            if (entry.surfaceType == SurfaceType.Default && entry.impactKind == ImpactKind.Default)
                fallbackDefault = entry;
        }

        return fallbackBySurface ?? fallbackDefault;
    }
}

[Serializable]
public class ImpactEffectEntry
{
    public SurfaceType surfaceType = SurfaceType.Default;
    public ImpactKind impactKind = ImpactKind.Default;

    public GameObject effectPrefab;
    public AudioClip[] soundClips;
    [Range(0f, 1f)] public float soundVolume = 1f;
    public float lifetime = 2f;
}