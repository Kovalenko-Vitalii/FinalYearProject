using UnityEngine;
using UnityEngine.UI;

public class SaveCurrentButton : MonoBehaviour
{
    [SerializeField] private Button saveButton;

    private void Awake()
    {
        if (saveButton == null)
            saveButton = GetComponent<Button>();

        saveButton.onClick.AddListener(Save);
    }

    private void Save()
    {
        var sm = SaveManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[Save] SaveManager missing");
            return;
        }

        if (string.IsNullOrEmpty(sm.CurrentSlotId))
        {
            Debug.LogWarning("[Save] No active save slot");
            return;
        }

        bool ok = sm.SaveToSlot(sm.CurrentSlotId);
        Debug.Log(ok
            ? $"[Save] Game saved to slot {sm.CurrentSlotId}"
            : "[Save] Save failed");
    }
}
