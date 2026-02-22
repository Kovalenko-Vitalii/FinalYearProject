using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [Header("List")]
    [SerializeField] private NoteQuestItemUI listItemPrefab;
    [SerializeField] private Transform listContent;

    [Header("Details")]
    [SerializeField] private NoteDetailsUI noteDetailsUI;
    [SerializeField] private QuestDetailsUI questDetailsUI;

    private void Start()
    {
        if (noteDetailsUI != null) noteDetailsUI.Hide();
        if (questDetailsUI != null) questDetailsUI.Hide();

        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        var active = QuestManager.Instance.ActiveQuest;
        if (active != null)
        {
            var item = Instantiate(listItemPrefab, listContent);
            item.BindQuest(active, OnQuestClicked);
        }

        var notes = NoteManager.Instance.GetNotes();
        foreach (var note in notes)
        {
            var item = Instantiate(listItemPrefab, listContent);
            item.BindNote(note, OnNoteClicked);
        }

        noteDetailsUI.Hide();
        questDetailsUI.Hide();
    }

    private void OnNoteClicked(NoteData note)
    {
        if (questDetailsUI != null) questDetailsUI.Hide();
        if (noteDetailsUI != null) noteDetailsUI.Show(note);

        SoundManager.Instance.PlayUI(UISoundId.NoteClick, note.onClickSound);
    }

    private void OnQuestClicked(QuestData quest)
    {
        if (noteDetailsUI != null) noteDetailsUI.Hide();
        if (questDetailsUI != null) questDetailsUI.Show(quest);

        SoundManager.Instance.PlayUI(UISoundId.NoteClick, null);
    }
}