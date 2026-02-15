using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteQuestItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;

    public void SetItem(NoteData noteData)
    {
        itemName.text = noteData.name;
    }
}
