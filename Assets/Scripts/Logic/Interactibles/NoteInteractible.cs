using System;
using UnityEngine;

public class NoteInteractible : MonoBehaviour, IInteractable, ISaveable
{
    [Header("Save")]
    [SerializeField] private string id;

    [Header("Data")]
    [SerializeField] private NoteData noteData;
    [SerializeField] private bool pickedUp = false;

    public bool PickedUp => pickedUp;
    public string SaveId => id;

    public void ApplyStateImmediate(bool picked)
    {
        pickedUp = picked;
        gameObject.SetActive(!pickedUp);
    }

    // --------- ISaveable ---------

    public object CaptureState()
    {
        return new NoteWorldState { pickedUp = pickedUp };
    }

    public void RestoreState(object state)
    {
        if (state is not NoteWorldState s) return;
        ApplyStateImmediate(s.pickedUp);
    }
    private void Reset()
    {
    #if UNITY_EDITOR
        SaveIdUtil.EnsureId(ref id, this);
    #else
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
    #endif
    }

    #if UNITY_EDITOR
    private void OnValidate() => SaveIdUtil.EnsureId(ref id, this);
    #endif


    public bool Interact(PlayerInteractor interactor)
    {
        if (pickedUp) return false;

        NoteManager.Instance.CollectNote(noteData);

        ApplyStateImmediate(true);

        SoundManager.Instance.PlayUI(UISoundId.NotePickupSound, noteData.onPickupSound);
        return true;
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = "Note: " + (noteData != null ? noteData.NoteName : "???");
        return true;
    }

    public void ResetToDefaultState()
    {
        
    }
}

[Serializable]
public struct NoteWorldState
{
    public bool pickedUp;
}