using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMiscUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] TextMeshProUGUI slotsAmountText;
    [SerializeField] private TextMeshProUGUI weightText;

    PlayerStatManager stats;
    InventoryManager invManager;

    private void Awake()
    {
        stats = PlayerStatManager.Instance;
        invManager = InventoryManager.Instance;
    }

    private void OnEnable()
    {
        if (stats == null)
            stats = PlayerStatManager.Instance;

        if (invManager == null)
            invManager = InventoryManager.Instance;

        if (stats == null || invManager == null)
        {
            Debug.LogWarning("InventoryMiscUI: managers not found.");
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = stats.MaxCarryWeight;

        stats.OnWeightChanged += UpdateWeightText;
        invManager.OnPlayerInventoryChanged += RefreshInventoryInfo;

        UpdateWeightText(stats.CurrentWeight);
        UpdateSlotsText(invManager.currentSlots, invManager.maxSlots);
    }

    private void OnDisable()
    {
        if (stats != null)
            stats.OnWeightChanged -= UpdateWeightText;

        if (invManager != null)
            invManager.OnPlayerInventoryChanged -= RefreshInventoryInfo;

    }

    private void RefreshInventoryInfo()
    {
        if (stats != null)
            UpdateWeightText(stats.CurrentWeight);

        UpdateSlotsText(invManager.currentSlots, invManager.maxSlots);
    }

    private void UpdateWeightText(float currentWeight)
    {
        if (slider == null || stats == null)
            return;

        slider.value = currentWeight;

        if (weightText != null)
            weightText.text = $"{currentWeight:0.0} / {stats.MaxCarryWeight:0.0} kg";
    }

    private void UpdateSlotsText(int currentSlots, int maxSlots)
    {
        if (slotsAmountText == null)
            return;

        slotsAmountText.text = maxSlots < 0
            ? $"{currentSlots}/∞"
            : $"{currentSlots}/{maxSlots}";
    }
}
