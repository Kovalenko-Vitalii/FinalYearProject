using UnityEngine;

// This scriptable object represents static data of note
[CreateAssetMenu(menuName = "Plot/NoteData")]
public class NoteData : ScriptableObject
{
    [Header("Base")]
    [SerializeField] public string id;
    [SerializeField] public string noteName;
    [SerializeField] Sprite icon;
    [SerializeField] Sprite background;
    [SerializeField, TextArea] string textContent;

    public AudioClip onClickSound;
    public AudioClip onPickupSound;

    public string Id => id;
    public string NoteName => noteName;
    public Sprite Icon => icon;
    public Sprite Background => background;
    public string TextContent => textContent;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = System.Guid.NewGuid().ToString();
    }
#endif
}
