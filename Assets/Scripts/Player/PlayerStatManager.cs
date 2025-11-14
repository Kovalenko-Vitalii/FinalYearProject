using UnityEngine;
using System;

public class PlayerStatManager : MonoBehaviour
{
    public static PlayerStatManager Instance { get; private set; }

    [Header("Main Stats")]
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float currentHunger = 100f;
    [SerializeField] private float currentHydration = 100f;
    [SerializeField] private float temperature = 36.6f;

    [Header("Energy / Fatigue")]
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float energyCap = 100f;

    [Header("Resistances")]
    [SerializeField] private float temperatureResist = 0;
    [SerializeField] private float damageResist = 0;

    [Header("Limits")]
    [SerializeField] private float healthCap = 100f;
    [SerializeField] private float hungerCap = 100f;
    [SerializeField] private float hydrationCap = 100f;

    public event Action<float> OnHealthChanged;
    public event Action<float> OnHungerChanged;
    public event Action<float> OnHydrationChanged;
    public event Action<float> OnTemperatureChanged;
    public event Action<float> OnEnergyChanged;
    public event Action<float> OnTemperatureResistChanged;
    public event Action<float> OnDamageResistChanged;

    public float Health => currentHealth;
    public float Hunger => currentHunger;
    public float Hydration => currentHydration;
    public float Temperature => temperature;
    public float Energy => currentEnergy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth);
        OnHungerChanged?.Invoke(currentHunger);
        OnHydrationChanged?.Invoke(currentHydration);
        OnTemperatureChanged?.Invoke(temperature);
        OnEnergyChanged?.Invoke(currentEnergy);
        OnTemperatureResistChanged?.Invoke(temperatureResist);
        OnDamageResistChanged?.Invoke(damageResist);
    }

    public void ChangeHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, healthCap);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void ChangeHunger(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, hungerCap);
        OnHungerChanged?.Invoke(currentHunger);
    }

    public void ChangeHydration(float amount)
    {
        currentHydration = Mathf.Clamp(currentHydration + amount, 0, hydrationCap);
        OnHydrationChanged?.Invoke(currentHydration);
    }

    public void ChangeTemperature(float amount)
    {
        temperature += amount;
        OnTemperatureChanged?.Invoke(temperature);
    }

    public void ChangeEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, energyCap);
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public void ApplyGear(GearData gear, int sign)
    {
        if (gear == null) return;

        if (gear.temperatureResist != 0f)
        {
            temperatureResist += gear.temperatureResist * sign;
            OnTemperatureResistChanged?.Invoke(temperatureResist);
        }

        if (gear.damageResist != 0f)
        {
            damageResist += gear.damageResist * sign;
            OnDamageResistChanged?.Invoke(damageResist);
        }
    }
}
