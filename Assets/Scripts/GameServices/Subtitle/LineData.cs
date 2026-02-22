using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Line Data", fileName = "Line")]

// Data class represents replica for subtitles
// Simply it is (what to say)
public class LineData : ScriptableObject
{
    // --- Should make it automatic
    public string id;

    [TextArea(2, 6)]
    public string text;
    public AudioClip voice;

    [Header("Timing (if no voice)")]
    public float duration = 1.2f;
}
