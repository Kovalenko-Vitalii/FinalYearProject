using UnityEngine;

[CreateAssetMenu(menuName = "Plot/NoteData")]

public class NoteData : ScriptableObject
{
    [Header("Base")]
    public string id;
    public string noteName;
    public Sprite icon;
    public Sprite background;
    [TextArea] public string textContent;
}
