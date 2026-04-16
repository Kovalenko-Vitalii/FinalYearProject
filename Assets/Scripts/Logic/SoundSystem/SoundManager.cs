using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const string MASTER_KEY = "audio_master";
    private const string UI_KEY = "audio_ui";
    private const string SUBTITLE_KEY = "audio_subtitle";
    private const string WORLD_KEY = "audio_world";

    [Header("World Audio")]
    [SerializeField] private AudioSource worldOneShotPrefab;
    [SerializeField, Range(0f, 1f)] private float volumeWorld = 1f;

    [Header("2D Audio Sources")]
    [SerializeField] AudioSource uiSource;
    [SerializeField] AudioSource subtitleSource;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float volumeMaster = 1f;
    [SerializeField, Range(0f, 1f)] float volumeUI = 1f;
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

    public float MasterVolume => volumeMaster;
    public float UIVolume => volumeUI;
    public float SubtitleVolume => volumeSubtitle;
    public float WorldVolume => volumeWorld;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (uiSource != null)
            uiSource.ignoreListenerPause = true;

        if (subtitleSource != null)
            subtitleSource.ignoreListenerPause = false;

        LoadVolumes();
        ApplyVolumes();
    }

    public void PausedSound(bool active) => AudioListener.pause = !active;

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
        source.ignoreListenerPause = false;
        source.pitch = pitch;
        source.volume = volumeMaster * volumeWorld;

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

    private void ApplyVolumes()
    {
        if (uiSource != null)
            uiSource.volume = volumeMaster * volumeUI;

        if (subtitleSource != null)
            subtitleSource.volume = volumeMaster * volumeSubtitle;
    }

    
    public void SetMasterVolume(float value)
    {
        volumeMaster = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetUIVolume(float value)
    {
        volumeUI = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetSubtitleVolume(float value)
    {
        volumeSubtitle = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetWorldVolume(float value)
    {
        volumeWorld = Mathf.Clamp01(value);
        SaveVolumes();
    }

    // Save/Load
    private void LoadVolumes()
    {
        volumeMaster = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        volumeUI = PlayerPrefs.GetFloat(UI_KEY, 1f);
        volumeSubtitle = PlayerPrefs.GetFloat(SUBTITLE_KEY, 1f);
        volumeWorld = PlayerPrefs.GetFloat(WORLD_KEY, 1f);
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MASTER_KEY, volumeMaster);
        PlayerPrefs.SetFloat(UI_KEY, volumeUI);
        PlayerPrefs.SetFloat(SUBTITLE_KEY, volumeSubtitle);
        PlayerPrefs.SetFloat(WORLD_KEY, volumeWorld);
        PlayerPrefs.Save();
    }
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

