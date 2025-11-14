using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [Header("Radial Bars")]
    [SerializeField] private Image temperatureBar;
    [SerializeField] private Image hungerBar;
    [SerializeField] private Image hydrationBar;
    [SerializeField] private Image energyBar;

    [Header("HP")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Image hpRadialBar;

    [Header("Refs")]
    [SerializeField] private PlayerStatManager stats;

    private void Awake()
    {
        if (stats == null)
            stats = PlayerStatManager.Instance;

        if (stats != null && hpBar != null)
        {
            hpBar.minValue = 0f;
            hpBar.maxValue = stats.HealthMax;
        }
    }

    private void OnEnable()
    {
        if (stats == null)
            stats = PlayerStatManager.Instance;

        if (stats == null)
        {
            Debug.LogWarning("StatsUI: no PlayerStatManager found.");
            return;
        }

        stats.OnHealthChanged += HandleHealthChanged;
        stats.OnHungerChanged += HandleHungerChanged;
        stats.OnHydrationChanged += HandleHydrationChanged;
        stats.OnEnergyChanged += HandleEnergyChanged;
        stats.OnTemperatureChanged += HandleTemperatureChanged;

        HandleHealthChanged(stats.Health);
        HandleHungerChanged(stats.Hunger);
        HandleHydrationChanged(stats.Hydration);
        HandleEnergyChanged(stats.Energy);
        HandleTemperatureChanged(stats.Temperature);
    }

    private void OnDisable()
    {
        if (stats == null) return;

        stats.OnHealthChanged -= HandleHealthChanged;
        stats.OnHungerChanged -= HandleHungerChanged;
        stats.OnHydrationChanged -= HandleHydrationChanged;
        stats.OnEnergyChanged -= HandleEnergyChanged;
        stats.OnTemperatureChanged -= HandleTemperatureChanged;
    }


    private void HandleHealthChanged(float value)
    {
        if (hpBar)
            hpBar.value = value;

        if (hpRadialBar && stats != null)
        {
            float t = Mathf.InverseLerp(0f, stats.HealthMax, value);
            hpRadialBar.fillAmount = t;
        }
    }

    private void HandleHungerChanged(float value)
    {
        if (!hungerBar || stats == null) return;
        float t = Mathf.InverseLerp(0f, stats.HungerMax, value);
        hungerBar.fillAmount = t;
    }

    private void HandleHydrationChanged(float value)
    {
        if (!hydrationBar || stats == null) return;
        float t = Mathf.InverseLerp(0f, stats.HydrationMax, value);
        hydrationBar.fillAmount = t;
    }

    private void HandleEnergyChanged(float value)
    {
        if (!energyBar || stats == null) return;
        float t = Mathf.InverseLerp(0f, stats.EnergyMax, value);
        energyBar.fillAmount = t;
    }

    private void HandleTemperatureChanged(float value)
    {
        if (!temperatureBar || stats == null) return;
        float t = Mathf.InverseLerp(stats.TemperatureMin, stats.TemperatureMax, value);
        temperatureBar.fillAmount = t;
    }
}
