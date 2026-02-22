using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteQuestItemUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Sprite questIcon;
    [SerializeField] Sprite noteIcon;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] Button button;

    private Action onClick;

    public void BindNote(NoteData noteData, Action<NoteData> clickCallback)
    {
        icon.sprite = noteIcon;
        itemName.text = noteData.NoteName;

        onClick = () => clickCallback?.Invoke(noteData);
        WireButton();
    }

    public void BindQuest(QuestData questData, Action<QuestData> clickCallback)
    {
        icon.sprite = questIcon;
        itemName.text = questData.title;

        onClick = () => clickCallback?.Invoke(questData);
        WireButton();
    }

    private void WireButton()
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }
}
