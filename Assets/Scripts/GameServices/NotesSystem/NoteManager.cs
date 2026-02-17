using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    [SerializeField] List<NoteData> notes = new();

    private NoteData[] _allNotesCache;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _allNotesCache = Resources.LoadAll<NoteData>("Notes");
    }

    public void CollectNote(NoteData note)
    {
        if (note == null) return;
        if (!notes.Contains(note))
            notes.Add(note);
    }

    public IReadOnlyList<NoteData> GetNotes() => notes;

    // Save load 

    public NotesSaveData Capture()
    {
        var data = new NotesSaveData();

        var world = Object.FindObjectsByType<NoteInteractible>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        data.worldNotes = world
            .Where(n => !string.IsNullOrEmpty(n.Id))
            .Select(n => new NoteSave { id = n.Id, pickedUp = n.PickedUp })
            .ToList();

        data.collectedNoteIds = notes
            .Where(n => n != null && !string.IsNullOrEmpty(n.id))
            .Select(n => n.id)
            .Distinct()
            .ToList();

        return data;
    }

    public void Restore(NotesSaveData data)
    {
        if (data == null) return;

        notes.Clear();

        var all = _allNotesCache != null && _allNotesCache.Length > 0
            ? _allNotesCache
            : Resources.LoadAll<NoteData>("Notes");

        foreach (var id in data.collectedNoteIds)
        {
            if (string.IsNullOrEmpty(id)) continue;

            NoteData found = null;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].id == id)
                {
                    found = all[i];
                    break;
                }
            }

            if (found != null)
                notes.Add(found);
        }

        var world = Object.FindObjectsByType<NoteInteractible>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        var map = world
            .Where(n => !string.IsNullOrEmpty(n.Id))
            .ToDictionary(n => n.Id, n => n);

        foreach (var s in data.worldNotes)
        {
            if (string.IsNullOrEmpty(s.id)) continue;
            if (map.TryGetValue(s.id, out var note))
                note.ApplyStateImmediate(s.pickedUp);
        }
    }
}
