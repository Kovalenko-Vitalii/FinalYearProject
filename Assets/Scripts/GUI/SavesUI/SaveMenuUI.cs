using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveMenuUI : MonoBehaviour
{
    [Header("Scroll View")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SaveSlotRowUI slotPrefab;

    [Header("Create new blank save")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button createButton;

    [Header("Blank save start location")]
    [SerializeField] private string startSceneName = "Level01";
    [SerializeField] private string startSpawnId = "Start";

    private void OnEnable()
    {
        Debug.Log("[SaveMenuUI] OnEnable");

        Debug.Log($"[SaveMenuUI] contentRoot={(contentRoot ? contentRoot.name : "NULL")}, slotPrefab={(slotPrefab ? slotPrefab.name : "NULL")}, createButton={(createButton ? createButton.name : "NULL")}");

        if (createButton != null)
        {
            createButton.onClick.RemoveListener(CreateBlankSave);
            createButton.onClick.AddListener(CreateBlankSave);
        }

        RefreshList();
    }

    private void OnDisable()
    {
        Debug.Log("[SaveMenuUI] OnDisable");

        if (createButton != null)
            createButton.onClick.RemoveListener(CreateBlankSave);
    }

    public void RefreshList()
    {
        Debug.Log("[SaveMenuUI] RefreshList");

        if (contentRoot == null)
        {
            Debug.LogError("[SaveMenuUI] contentRoot is NULL (assign ScrollView/Viewport/Content)");
            return;
        }
        if (slotPrefab == null)
        {
            Debug.LogError("[SaveMenuUI] slotPrefab is NULL (assign slot row prefab)");
            return;
        }

        // Clear
        Debug.Log($"[SaveMenuUI] Clearing content children: {contentRoot.childCount}");
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        var sm = SaveManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SaveMenuUI] SaveManager.Instance is NULL. Is SaveManager in Core scene and active?");
            return;
        }

        var slots = sm.ListSlots();
        Debug.Log($"[SaveMenuUI] ListSlots returned: {(slots == null ? "NULL" : slots.Length.ToString())}");

        if (slots == null) return;

        foreach (var meta in slots)
        {
            Debug.Log($"[SaveMenuUI] Creating row for slot id={meta.id}, name={meta.displayName}");
            var row = Instantiate(slotPrefab, contentRoot);
            row.Bind(meta, this);
        }

        Debug.Log($"[SaveMenuUI] After populate, content children: {contentRoot.childCount}");
    }

    public void LoadSlot(string slotId)
    {
        Debug.Log($"[SaveMenuUI] LoadSlot clicked: {slotId}");

        var sm = SaveManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SaveMenuUI] SaveManager.Instance is NULL on LoadSlot");
            return;
        }

        bool ok = sm.LoadSlot(slotId);
        Debug.Log($"[SaveMenuUI] LoadSlot result: {ok}");
    }

    private void CreateBlankSave()
    {
        Debug.Log("[SaveMenuUI] CreateBlankSave clicked");

        var sm = SaveManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SaveMenuUI] SaveManager.Instance is NULL on CreateBlankSave");
            return;
        }

        string displayName = (nameInput != null) ? nameInput.text : "";
        Debug.Log($"[SaveMenuUI] nameInput={(nameInput ? "OK" : "NULL")}, text='{displayName}'");

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = "New Save";

        string id = sm.CreateBlankSlot(displayName, startSceneName, startSpawnId);
        Debug.Log($"[SaveMenuUI] Created blank slot id={id}");

        if (nameInput != null) nameInput.text = "";

        RefreshList();
    }
}
