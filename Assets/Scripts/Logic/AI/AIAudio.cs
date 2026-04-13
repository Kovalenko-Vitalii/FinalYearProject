using System.Collections.Generic;
using UnityEngine;

// This class plays sounds of AI creature
[RequireComponent(typeof(AudioSource))]
public class AIAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Configured sounds")]
    [SerializeField] private List<AISoundEntry> sounds = new();

    private readonly Dictionary<AISoundType, AISoundEntry> soundMap = new();
    private readonly Dictionary<AISoundType, float> lastPlayTime = new();

    private void Reset() => audioSource = GetComponent<AudioSource>();
    private void Awake() => RebuildMap();
    private void OnValidate() => RebuildMap();

    public bool HasSound(AISoundType type)
    {
        return soundMap.ContainsKey(type);
    }

    public bool Play(AISoundType type)
    {
        if (audioSource == null)
            return false;

        if (!soundMap.TryGetValue(type, out AISoundEntry entry) || entry == null)
        {
            Debug.LogWarning($"{name}: sound type '{type}' is not configured.", this);
            return false;
        }

        if (entry.clips == null || entry.clips.Length == 0)
        {
            Debug.LogWarning($"{name}: sound type '{type}' has no clips.", this);
            return false;
        }

        if (entry.cooldown > 0f &&
            lastPlayTime.TryGetValue(type, out float lastTime) &&
            Time.time - lastTime < entry.cooldown)
        {
            return false;
        }

        AudioClip clip = entry.clips[Random.Range(0, entry.clips.Length)];
        if (clip == null)
            return false;

        audioSource.pitch = Random.Range(entry.pitchRange.x, entry.pitchRange.y);

        float volume = Random.Range(entry.volumeRange.x, entry.volumeRange.y);
        audioSource.PlayOneShot(clip, volume);

        lastPlayTime[type] = Time.time;
        return true;
    }

    private void RebuildMap()
    {
        soundMap.Clear();

        if (sounds == null)
            return;

        foreach (AISoundEntry entry in sounds)
        {
            if (entry == null)
                continue;

            if (soundMap.ContainsKey(entry.type))
            {
                Debug.LogWarning($"{name}: duplicate sound type '{entry.type}' in AIAudio.", this);
                continue;
            }

            soundMap.Add(entry.type, entry);
        }
    }
}

public enum AISoundType
{
    Hurt,
    Death,
    Footstep,
    Idle,

    Investigate,
    ChaseStart,
    Attack,
    Roar,

    Alert,
    Suspicion,
    Search
}

[System.Serializable]
public class AISoundEntry
{
    public AISoundType type;
    public AudioClip[] clips;

    [Header("Randomization")]
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    public Vector2 volumeRange = new Vector2(1f, 1f);

    [Header("Optional")]
    public float cooldown = 0f;
}