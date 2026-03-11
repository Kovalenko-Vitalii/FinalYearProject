using UnityEngine;

// This class plays sounds of AI creature
[RequireComponent(typeof(AudioSource))]
public class AIAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AIHealth health;

    [Header("Clips")]
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip deathClip;

    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip[] idleClips;

    [Header("Random Pitch")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private void Reset() 
    {
        health = GetComponent<AIHealth>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (health == null)
            return;

        health.Damaged += PlayHurt;
        health.Died += PlayDeath;
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.Damaged -= PlayHurt;
        health.Died -= PlayDeath;
    }

    public void PlayHurt() => PlayOneShot(hurtClip);
    
    public void PlayDeath() => PlayOneShot(deathClip);

    public void PlayFootstep() => PlayRandomSound(footstepClips);

    public void PlayIdle() => PlayRandomSound(idleClips);

    public void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayOneShot(clip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }
}