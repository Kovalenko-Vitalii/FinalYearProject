using UnityEngine;

public class NoteInteractible : MonoBehaviour, IInteractable
{
    public string id;
    [SerializeField] NoteData noteData;
    public bool pickedUp = false;

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

        gameObject.SetActive(false);

        SoundManager.Instance.PlayUI(UISoundId.NotePickupSound, noteData.onPickupSound);

        pickedUp = true;

        return true;
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = "Note: " + noteData.NoteName;
        return true;
    }
}
