using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] AudioSource audioSource;

    // List of sounds
    [SerializeField] List<AudioClip> sounds;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PlaySound(string soundName)
    {
        foreach (var sound in sounds)
        {
            if (sound.name == soundName)
                audioSource.PlayOneShot(sound);
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

}
