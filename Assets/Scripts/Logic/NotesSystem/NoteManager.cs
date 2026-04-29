using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This class represents a service that stores picked up notes
public class NoteManager : MonoBehaviour, ISaveable
{
    public static NoteManager Instance { get; private set; }

    [SerializeField] private List<NoteData> notes = new();

    private NoteData[] _allNotesCache;
    public string SaveId => "NOTE_MANAGER";

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

    // -------- ISaveable --------

    public object CaptureState()
    {
        var ids = notes
            .Where(n => n != null && !string.IsNullOrEmpty(n.id))
            .Select(n => n.id)
            .Distinct()
            .ToList();

        return new NotesCollectedState { collectedNoteIds = ids };
    }

    public void RestoreState(object state)
    {
        if (state is not NotesCollectedState s) return;

        notes.Clear();

        var all = (_allNotesCache != null && _allNotesCache.Length > 0)
            ? _allNotesCache
            : Resources.LoadAll<NoteData>("Notes");

        if (s.collectedNoteIds == null) return;

        foreach (var id in s.collectedNoteIds)
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
    }

    public void ResetToDefaultState()
    {
        notes.Clear();
    }
}

[Serializable]
public struct NotesCollectedState
{
    public List<string> collectedNoteIds;
}