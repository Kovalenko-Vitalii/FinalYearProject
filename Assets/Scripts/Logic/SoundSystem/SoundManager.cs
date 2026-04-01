using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("World Audio")]
    [SerializeField] private AudioSource worldOneShotPrefab;
    [SerializeField, Range(0f, 1f)] private float volumeWorld = 1f;

    [Header("2D Audio Sources")]
    [SerializeField] AudioSource uiSource;
    [SerializeField] AudioSource footstepSource;
    [SerializeField] AudioSource subtitleSource;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] float volumeUI = 1f;
    [SerializeField, Range(0f, 1f)] float volumeFootstep = 1f;
    [SerializeField, Range(0f, 1f)] float volumeSubtitle = 1f;

    [Header("Default Sounds")]
    [SerializeField] AudioClip uiClick;
    [SerializeField] AudioClip itemClick;
    [SerializeField] AudioClip menuOpen;
    [SerializeField] AudioClip menuClose;
    [SerializeField] AudioClip dropItem;
    [SerializeField] AudioClip equipItem;
    [SerializeField] AudioClip unequipItem;
    [SerializeField] AudioClip consumeItem;
    [SerializeField] AudioClip pickupItem;
    [SerializeField] AudioClip rejectSound;
    [SerializeField] AudioClip notePickupSound;
    [SerializeField] AudioClip noteClick;
    [SerializeField] AudioClip deathSound;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayUI(UISoundId id, AudioClip overrideClip = null)
    {
        if (uiSource == null) return;

        var clip = overrideClip != null ? overrideClip : GetDefault(id);
        if (clip == null) return;

        uiSource.PlayOneShot(clip, volumeUI);
    }

    public void PlayUI(AudioClip clip)
    {
        if (uiSource == null || !clip) return;
        uiSource.PlayOneShot(clip, volumeUI);
    }

    public void PlayFootstep(AudioClip clip, float volumeMul = 1f, float pitch = 1f)
    {
        if (footstepSource == null || clip == null) return;

        footstepSource.pitch = pitch;
        footstepSource.PlayOneShot(clip, volumeFootstep * volumeMul);
    }

    public void PlaySubtitleSound(AudioClip clip)
    {
        if (!subtitleSource || !clip) return;
        subtitleSource.PlayOneShot(clip, volumeSubtitle);
    }

    public void PlayWorldOneShot(AudioClip clip, Vector3 position, float volumeMul = 1f, float pitch = 1f)
    {
        if (clip == null)
            return;

        if (worldOneShotPrefab == null)
            return;

        AudioSource source = Instantiate(worldOneShotPrefab, position, Quaternion.identity);
        source.pitch = pitch;
        source.PlayOneShot(clip, volumeWorld * volumeMul);

        float lifetime = clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
        Destroy(source.gameObject, lifetime + 0.1f);
    }

    private AudioClip GetDefault(UISoundId id) => id switch
    {
        UISoundId.UIClick => uiClick,
        UISoundId.ItemClick => itemClick,
        UISoundId.MenuOpen => menuOpen,
        UISoundId.MenuClose => menuClose,
        UISoundId.DropItem => dropItem,
        UISoundId.EquipItem => equipItem,
        UISoundId.UnequipItem => unequipItem,
        UISoundId.ConsumeItem => consumeItem,
        UISoundId.PickupSound => pickupItem,
        UISoundId.RejectSound => rejectSound,
        UISoundId.NotePickupSound => notePickupSound,
        UISoundId.NoteClick => noteClick,
        UISoundId.DeathSound => deathSound,
        _ => null
    };
}

public enum UISoundId
{
    UIClick,
    ItemClick,
    MenuOpen,
    MenuClose,
    DropItem,
    EquipItem,
    UnequipItem,
    ConsumeItem,
    PickupSound,
    RejectSound,
    NotePickupSound,
    NoteClick,
    DeathSound
}

