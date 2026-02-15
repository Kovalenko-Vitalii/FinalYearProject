using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField] NoteQuestItemUI listItemPrefab;
    [SerializeField] Transform listContent;
    [SerializeField] private NoteDetailsUI detailsUI;
    private void Start()
    {
        Refresh();
        if (detailsUI != null) detailsUI.Hide();
    }

    public void Refresh()
    {
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        var notes = NoteManager.Instance.GetNotes();

        foreach (var note in notes)
        {
            var item = Instantiate(listItemPrefab, listContent);
            item.Bind(note, OnItemClicked);
        }
    }

    private void OnItemClicked(NoteData note)
    {
        if (detailsUI != null)
            detailsUI.Show(note);
        SoundManager.Instance.PlayUI(UISoundId.NoteClick, note.onClickSound);
    }
}
