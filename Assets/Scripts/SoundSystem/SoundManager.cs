using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] AudioSource uiSource;
    [SerializeField, Range(0f, 1f)] float volumeUI = 1f;

    [Header("Default Sounds")]
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip itemClick;
    [SerializeField] private AudioClip menuOpen;
    [SerializeField] private AudioClip menuClose;
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

    private AudioClip GetDefault(UISoundId id) => id switch
    {
        UISoundId.UIClick => uiClick,
        UISoundId.ItemClick => itemClick,
        UISoundId.MenuOpen => menuOpen,
        UISoundId.MenuClose => menuClose,
        _ => null
    };
}

public enum UISoundId
{
    UIClick,
    ItemClick,
    MenuOpen,
    MenuClose
}

