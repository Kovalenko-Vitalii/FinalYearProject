using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GearSelectionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image centerIcon;
    [SerializeField] private Image leftPreviewIcon;
    [SerializeField] private Image rightPreviewIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Button leftBtn;
    [SerializeField] private Button rightBtn;
    [SerializeField] private Button equipBtn;
    [SerializeField] private GameObject centerHighlight;

    [Header("Preview")]
    [SerializeField] private Color previewDimColor = new Color(1, 1, 1, 0.35f);
    [SerializeField] private Color previewHiddenColor = new Color(1, 1, 1, 0f);

    [Header("Stats UI")]
    [SerializeField] private Transform statRoot;
    [SerializeField] private StatWidget statPrefab;
    [SerializeField] private StatLibrary statLibrary;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    private readonly List<StatWidget> statPool = new();

    private List<GearData> options = new();
    private int index = -1;

    private GearData.GearSlot activeSlot;
    private GearData equipped;
    private Inventory playerInv;
    private Equipment playerEq;

    private void Awake()
    {
        if (leftBtn) leftBtn.onClick.AddListener(OnLeft);
        if (rightBtn) rightBtn.onClick.AddListener(OnRight);
        if (equipBtn) equipBtn.onClick.AddListener(OnEquip);
    }

    public void Open(GearData.GearSlot slot)
    {
        var im = InventoryManager.Instance;
        playerInv = im.playerInventory;
        playerEq = im.playerEquipment;

        activeSlot = slot;
        equipped = playerEq.GetEquipped(slot);

        BuildOptions();
        UpdateUI();
    }


    private void OnLeft()
    {
        if (options.Count == 0) return;
        if (index > 0) index--;
        UpdateUI();
    }

    private void OnRight()
    {
        if (options.Count == 0) return;
        if (index < options.Count - 1) index++;
        UpdateUI();
    }

    private void OnEquip()
    {
        if (options.Count == 0 || index < 0) return;

        var selected = options[index];

        // Нажали на уже надетый – ничего не делаем
        if (equipped == selected)
            return;

        // Перемещение: из инвентаря -> в слот
        playerInv.RemoveItem(selected, 1);   // убрать из инвентаря

        var oldGear = playerEq.Equip(selected); // надеть выбранный
        if (oldGear != null)
            playerInv.AddItem(oldGear, 1);      // старый вернуть в инвентарь

        equipped = selected; // теперь это надетый

        // Пересобрать список (первым – надетый, остальное – то, что осталось в инвентаре)
        BuildOptions();

        // Обновить остальной UI
        var gearUI = Object.FindAnyObjectByType<GearUI>();
        if (gearUI) gearUI.Refresh();
        foreach (var invUI in Object.FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
            invUI.Refresh();

        UpdateUI();
    }


    public void UpdateUI()
    {
        bool hasList = options.Count > 0 && index >= 0;
        if (leftBtn) leftBtn.interactable = hasList && index > 0;
        if (rightBtn) rightBtn.interactable = hasList && index < options.Count - 1;

        if (!hasList)
        {
            SetImage(centerIcon, equipped?.icon, Color.white);
            nameText.text = equipped != null ? equipped.itemName : "Nothing equipped";
            descText.text = equipped != null ? equipped.description : "Нет доступных предметов.";
            SetImage(leftPreviewIcon, null, previewHiddenColor);
            SetImage(rightPreviewIcon, null, previewHiddenColor);
            if (centerHighlight) centerHighlight.SetActive(equipped != null);
            RenderStats(equipped); // статы экипнутого
            return;
        }

        var sel = options[index];
        SetImage(centerIcon, sel.icon, Color.white);
        nameText.text = sel.itemName;
        descText.text = sel.description;

        if (index > 0)
            SetImage(leftPreviewIcon, options[index - 1].icon, previewDimColor);
        else
            SetImage(leftPreviewIcon, null, previewHiddenColor);

        if (index < options.Count - 1)
            SetImage(rightPreviewIcon, options[index + 1].icon, previewDimColor);
        else
            SetImage(rightPreviewIcon, null, previewHiddenColor);

        if (centerHighlight) centerHighlight.SetActive(equipped != null && ReferenceEquals(sel, equipped));

        // статы выбранного в центре
        RenderStats(sel);
    }

    private void SetImage(Image img, Sprite sprite, Color tint)
    {
        if (!img) return;
        img.sprite = sprite;
        img.color = tint;
        img.enabled = sprite != null || tint.a > 0f;
    }

    // ----- СТАТЫ (без сравнения) -----
    private void RenderStats(ItemData itemData)
    {
        // скрыть все виджеты из пула
        foreach (var w in statPool) w.gameObject.SetActive(false);

        if (itemData is not IStatProvider provider || statRoot == null || statPrefab == null || statLibrary == null)
        {
            ForceRebuild();
            return;
        }

        var stats = provider.GetStats()
            .Select(s => (s, desc: statLibrary.Get(s.id)))
            .Where(t => !(t.desc.hideIfZero && Mathf.Approximately(t.s.value, 0)))
            .OrderBy(t => t.desc.priority)
            .ToList();

        foreach (var t in stats)
        {
            var w = GetStatWidget();
            var formatted = t.desc.Format(t.s.value) + (string.IsNullOrEmpty(t.desc.unit) ? "" : $" {t.desc.unit}");
            float? diff = null; // без сравнения
            w.Bind(t.desc.icon, formatted, diff, positiveColor, negativeColor);
        }

        ForceRebuild();
    }

    private StatWidget GetStatWidget()
    {
        var w = statPool.FirstOrDefault(x => !x.gameObject.activeSelf);
        if (w == null)
        {
            w = Instantiate(statPrefab, statRoot);
            statPool.Add(w);
        }
        return w;
    }

    private void ForceRebuild()
    {
        if (statRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)statRoot);
    }

    private void BuildOptions()
    {
        // первым – экипнутый (если есть)
        options = new List<GearData>();
        if (equipped != null && equipped.slot == activeSlot)
            options.Add(equipped);

        // далее – все подходящие из инвентаря (можно оставить Distinct, либо развернуть по amount)
        var fromInv = playerInv.items
            .Where(i => i.data is GearData g && g.slot == activeSlot)
            .Select(i => (GearData)i.data)
            .Where(g => g != equipped); // не дублируем экипнутый

        options.AddRange(fromInv.Distinct());
        index = options.Count > 0 ? 0 : -1;
    }

}
