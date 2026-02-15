using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField] GameObject listItemPrefab;
    [SerializeField] Transform listContent;
    
    public void Refresh()
    {
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        var notes = NoteManager.Instance.GetNotes();

        foreach (var note in notes)
        {
            var obj = Instantiate(listItemPrefab, listContent);
            obj.GetComponent<NoteQuestItemUI>().SetItem(note);
        }
    }
}
