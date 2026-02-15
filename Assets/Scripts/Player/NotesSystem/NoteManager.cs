using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    [SerializeField] List<NoteData> notes = new List<NoteData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CollectNote(NoteData note) { notes.Add(note); }
       
    public IReadOnlyList<NoteData> GetNotes() { return notes; }
}
