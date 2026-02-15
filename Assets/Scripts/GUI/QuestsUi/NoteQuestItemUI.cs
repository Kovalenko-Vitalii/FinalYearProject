using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteQuestItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private Button button;

    private NoteData data;
    private Action<NoteData> onClick;

    public void Bind(NoteData noteData, Action<NoteData> clickCallback)
    {
        data = noteData;
        onClick = clickCallback;

        itemName.text = noteData.NoteName;
        if (icon != null) icon.sprite = noteData.Icon;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(data));
        }
    }
}
