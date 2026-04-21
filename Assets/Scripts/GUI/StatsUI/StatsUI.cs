using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [Header("Radial Bars")]
    [SerializeField] private Image hungerBar;
    [SerializeField] private Image hydrationBar;
    [SerializeField] private Image energyBar;
    [SerializeField] private Image temperatureBar;


    [Header("HP")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Image hpRadialBar;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;


    private PlayerStatManager stats;

    private void OnEnable()
    {
        var orch = GameplayOrchestrator.Instance;
        if (orch != null)
        {
            orch.OnPlayerSpawned += HandlePlayerSpawned;
            orch.OnEnterMenu += HandleEnterMenu;
            orch.OnGameplayReady += HandleGameplayReady;
        }

        TryBindNow();
    }

    private void OnDisable()
    {
        UnbindStats();

        var orch = GameplayOrchestrator.Instance;
        if (orch != null)
        {
            orch.OnPlayerSpawned -= HandlePlayerSpawned;
            orch.OnEnterMenu -= HandleEnterMenu;
            orch.OnGameplayReady -= HandleGameplayReady;
        }
    }

    private void HandlePlayerSpawned(GameObject player)
    {
        TryBindNow();
    }

    private void HandleGameplayReady()
    {
        TryBindNow();
    }

    private void HandleEnterMenu()
    {
        UnbindStats();
    }

    private void TryBindNow()
    {
        if (stats != null) return;

        stats = PlayerStatManager.Instance;
        if (stats == null) return;

        BindStats(stats);
    }

    private void BindStats(PlayerStatManager s)
    {
        stats = s;

        if (hpBar != null)
        {
            hpBar.minValue = 0f;
            hpBar.maxValue = stats.HealthMax;
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

    private void UnbindStats()
    {
        if (stats == null) return;

        stats.OnHealthChanged -= HandleHealthChanged;
        stats.OnHungerChanged -= HandleHungerChanged;
        stats.OnHydrationChanged -= HandleHydrationChanged;
        stats.OnEnergyChanged -= HandleEnergyChanged;
        stats.OnTemperatureChanged -= HandleTemperatureChanged;

        stats = null;
    }

    private void HandleHealthChanged(float value)
    {
        if (hpBar) hpBar.value = value;

        if (hpRadialBar && stats != null)
        {
            float t = Mathf.InverseLerp(0f, stats.HealthMax, value);
            hpRadialBar.fillAmount = t;
            hpBar.fillRect.GetComponent<Image>().color = GetStatColor(t);
        }
    }

    private Color GetStatColor(float t)
    {
        if (t < 0.33f) return criticalColor;
        if (t < 0.66f) return warningColor;
        return normalColor;
    }

    private void HandleHungerChanged(float value)
    {
        if (!hungerBar || stats == null) return;
        float t = Mathf.InverseLerp(0f, stats.HungerMax, value);
        hungerBar.fillAmount = t;
        hungerBar.color = GetStatColor(t);
    }

    private void HandleHydrationChanged(float value)
    {
        if (!hydrationBar || stats == null) return;
        float t = Mathf.InverseLerp(0f, stats.HydrationMax, value);
        hydrationBar.fillAmount = t;
        hydrationBar.color = GetStatColor(t);
    }

    private void HandleEnergyChanged(float value)
    {
        if (!energyBar || stats == null) return;
        float t = Mathf.InverseLerp(0f, stats.EnergyMax, value);
        energyBar.fillAmount = t;
        energyBar.color = GetStatColor(t);
    }

    private void HandleTemperatureChanged(float value)
    {
        if (temperatureBar == null || stats == null) return;

        float t = Mathf.InverseLerp(0f, stats.TemperatureMax, value);
        temperatureBar.fillAmount = t;
        temperatureBar.color = GetStatColor(t);
    }
}
