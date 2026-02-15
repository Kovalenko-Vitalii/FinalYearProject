using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteDetailsUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI content;

    public void Show(NoteData note)
    {
        if (note == null) return;

        if (root != null) root.SetActive(true);
        if (background != null) background.sprite = note.Background;
        if (title != null) title.text = note.NoteName;
        if (content != null) content.text = note.TextContent;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
