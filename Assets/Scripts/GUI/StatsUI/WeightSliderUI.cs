using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeightSliderUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private PlayerStatManager stats;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (stats == null)
            stats = PlayerStatManager.Instance;
    }

    private void OnEnable()
    {
        if (stats == null)
        {
            stats = PlayerStatManager.Instance;
            if (stats == null)
            {
                Debug.LogWarning("WeightSliderUI: PlayerStatManager not found.");
                return;
            }
        }

        slider.minValue = 0f;
        slider.maxValue = stats.MaxCarryWeight;

        stats.OnWeightChanged += UpdateUI;

        UpdateUI(stats.CurrentWeight);
    }

    private void OnDisable()
    {
        if (stats != null)
            stats.OnWeightChanged -= UpdateUI;
    }

    private void UpdateUI(float current)
    {
        if (!slider) return;

        slider.value = current;

        if (weightText)
            weightText.text = $"{current:0.0} / {stats.MaxCarryWeight:0.0} кг";
    }
}
