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
    [SerializeField] private Toggle isTestLocation;

    [Header("Default start location")]
    [SerializeField] private string startSceneName = "Level01";
    [SerializeField] private string startSpawnId = "level01";

    [Header("Test start location")]
    [SerializeField] private string testSceneName = "TestLevel";
    [SerializeField] private string testSpawnId = "TestStart";

    private void OnEnable()
    {
        if (createButton != null)
        {
            createButton.onClick.RemoveListener(CreateBlankSave);
            createButton.onClick.AddListener(CreateBlankSave);
        }

        RefreshList();
    }

    private void OnDisable()
    {
        if (createButton != null)
            createButton.onClick.RemoveListener(CreateBlankSave);
    }

    public void RefreshList()
    {
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

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        var sm = SaveManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SaveMenuUI] SaveManager.Instance is NULL. Is SaveManager in Core scene and active?");
            return;
        }

        var slots = sm.ListSlots();
        if (slots == null) return;

        foreach (var meta in slots)
        {
            Debug.Log($"[SaveMenuUI] Creating row for slot id={meta.id}, name={meta.displayName}");
            var row = Instantiate(slotPrefab, contentRoot);
            row.Bind(meta, this);
        }
    }

    public void LoadSlot(string slotId)
    {
        var sm = SaveManager.Instance;
        if (sm == null)
            return;

        bool ok = sm.LoadSlot(slotId);

        if (!ok)
            Debug.LogError($"[SaveMenuUI] Failed to load slot '{slotId}'");
    }

    private void CreateBlankSave()
    {
        var sm = SaveManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SaveMenuUI] SaveManager.Instance is NULL on CreateBlankSave");
            return;
        }

        string displayName = (nameInput != null) ? nameInput.text : "";

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = "New Save";

        bool useTest = isTestLocation != null && isTestLocation.isOn;

        string sceneName = useTest ? testSceneName : startSceneName;
        string spawnId = useTest ? testSpawnId : startSpawnId;

        string id = sm.CreateBlankSlot(displayName, sceneName, spawnId);

        Debug.Log($"[SaveMenuUI] Blank save created. id={id}, scene={sceneName}, spawn={spawnId}, test={useTest}");

        if (nameInput != null)
            nameInput.text = "";

        RefreshList();
    }
}