using UnityEngine;

public class NoteInteractible : MonoBehaviour, IInteractable
{
    public string id;
    [SerializeField] NoteData noteData;
    public bool pickedUp = false;

    public string Id => id;
    public bool PickedUp => pickedUp;

    public void ApplyStateImmediate(bool picked)
    {
        pickedUp = picked;
        gameObject.SetActive(!pickedUp);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            GenerateId();
            return;
        }

        var all = FindObjectsByType<NoteInteractible>(FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c == this) continue;
            if (c.id == id)
            {
                GenerateId();
                break;
            }
        }
    }

    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString("N");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    public bool Interact(PlayerInteractor interactor)
    {
        NoteManager.Instance.CollectNote(noteData);

        pickedUp = true;
        gameObject.SetActive(false);

        SoundManager.Instance.PlayUI(UISoundId.NotePickupSound, noteData.onPickupSound);
        return true;
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = "Note: " + noteData.NoteName;
        return true;
    }
}
