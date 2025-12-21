using UnityEngine;
using System;
using static GameplayOrchestrator;

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

    [Header("Carried Weight")]
    [SerializeField] private float baseWeight = 0f;
    [SerializeField] private float currentWeight = 0f;
    [SerializeField] private float maxCarryWeight = 35f;

    [Header("Resistances")]
    [SerializeField] private float temperatureResist = 0;
    [SerializeField] private float damageResist = 0;

    [Header("Limits")]
    [SerializeField] private float healthCap = 100f;
    [SerializeField] private float hungerCap = 100f;
    [SerializeField] private float hydrationCap = 100f;

    [Header("Temperature Limits")]
    [SerializeField] private float minTemperature = 30f;
    [SerializeField] private float maxTemperature = 42f;

    [Header("Natural Change Rates (per second)")]
    [SerializeField] private float healthRegenPerSecond = 0f; 
    [SerializeField] private float healthDegenerationPerSecond = 0f;

    [SerializeField] private float hungerDrainPerSecond = 0.2f;
    [SerializeField] private float hydrationDrainPerSecond = 0.3f;

    [SerializeField] private float energyDrainPerSecond = 1f;
    [SerializeField] private float energyRegenPerSecond = 5f;

    [SerializeField] private float temperatureChangeTowardsNormalPerSecond = 0.1f; 
    [SerializeField] private float normalTemperature = 36.6f;

    [Header("Rate Multipliers (runtime)")]
    [SerializeField] private float hungerRateMultiplier = 1f;
    [SerializeField] private float hydrationRateMultiplier = 1f;
    [SerializeField] private float energyRateMultiplier = 1f;
    [SerializeField] private float healthRegenMultiplier = 1f;
    [SerializeField] private float temperatureRateMultiplier = 1f;

    // Events
    public event Action<float> OnHealthChanged;
    public event Action<float> OnHungerChanged;
    public event Action<float> OnHydrationChanged;
    public event Action<float> OnTemperatureChanged;
    public event Action<float> OnEnergyChanged;
    public event Action<float> OnTemperatureResistChanged;
    public event Action<float> OnDamageResistChanged;
    public event Action<float> OnWeightChanged;

    public float Health => currentHealth;
    public float Hunger => currentHunger;
    public float Hydration => currentHydration;
    public float Temperature => temperature;
    public float Energy => currentEnergy;

    public float Weight => currentWeight + baseWeight;
    public float CurrentWeight => Weight;
    public float MaxCarryWeight => maxCarryWeight;

    public float HealthMax => healthCap;
    public float HungerMax => hungerCap;
    public float HydrationMax => hydrationCap;
    public float EnergyMax => energyCap;
    public float TemperatureMin => minTemperature;
    public float TemperatureMax => maxTemperature;

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
        var invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            invMgr.OnPlayerInventoryChanged += HandleInventoryChanged;

            if (invMgr.playerEquipment != null)
                invMgr.playerEquipment.OnChanged += HandleEquipmentChanged;
        }

        OnHealthChanged?.Invoke(currentHealth);
        OnHungerChanged?.Invoke(currentHunger);
        OnHydrationChanged?.Invoke(currentHydration);
        OnTemperatureChanged?.Invoke(temperature);
        OnEnergyChanged?.Invoke(currentEnergy);
        OnTemperatureResistChanged?.Invoke(temperatureResist);
        OnDamageResistChanged?.Invoke(damageResist);

        RecalculateWeight();
    }

    private void OnDestroy()
    {
        var invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            invMgr.OnPlayerInventoryChanged -= HandleInventoryChanged;

            if (invMgr.playerEquipment != null)
                invMgr.playerEquipment.OnChanged -= HandleEquipmentChanged;
        }
    }

    private void Update()
    {
        if (GameplayOrchestrator.Instance.State != GameState.Gameplay) return;
        if (PauseManager.Instance.IsPaused) return;

        float dt = Time.deltaTime;
        TickNaturalStats(dt);
    }

    public void ApplyConsumable(ConsumableData cd)
    {
        if (cd == null) return;

        if (cd.hpRestore != 0)
            ChangeHealth(cd.hpRestore);

        if (cd.hungerRestore != 0)
            ChangeHunger(cd.hungerRestore);

        if (cd.hydrationRestore != 0)
            ChangeHydration(cd.hydrationRestore);

        if (cd.temperatureRestore != 0)
            ChangeTemperature(cd.temperatureRestore);
    }


    public void ChangeHealth(float amount)
    {
        float old = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, healthCap);
        if (!Mathf.Approximately(old, currentHealth))
            OnHealthChanged?.Invoke(currentHealth);
    }

    public void ChangeHunger(float amount)
    {
        float old = currentHunger;
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, hungerCap);
        if (!Mathf.Approximately(old, currentHunger))
            OnHungerChanged?.Invoke(currentHunger);
    }

    public void ChangeHydration(float amount)
    {
        float old = currentHydration;
        currentHydration = Mathf.Clamp(currentHydration + amount, 0, hydrationCap);
        if (!Mathf.Approximately(old, currentHydration))
            OnHydrationChanged?.Invoke(currentHydration);
    }

    public void ChangeTemperature(float amount)
    {
        float old = temperature;
        temperature = Mathf.Clamp(temperature + amount, minTemperature, maxTemperature);
        if (!Mathf.Approximately(old, temperature))
            OnTemperatureChanged?.Invoke(temperature);
    }

    public void ChangeEnergy(float amount)
    {
        float old = currentEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, energyCap);
        if (!Mathf.Approximately(old, currentEnergy))
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

    private void HandleInventoryChanged()
    {
        RecalculateWeight();
    }

    private void HandleEquipmentChanged(GearData.GearSlot slot, GearData oldGear, GearData newGear)
    {
        RecalculateWeight();
    }

    private void RecalculateWeight()
    {
        float total = 0f;

        var invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            var inv = invMgr.playerInventory;
            if (inv != null && inv.items != null)
            {
                foreach (var item in inv.items)
                {
                    if (item?.data == null) continue;
                    total += item.data.weight * item.amount;
                }
            }

            var eq = invMgr.playerEquipment;
            if (eq != null)
            {
                AddGearWeightIfNotNull(eq.GetEquipped(GearData.GearSlot.Head), ref total);
                AddGearWeightIfNotNull(eq.GetEquipped(GearData.GearSlot.Chest), ref total);
                AddGearWeightIfNotNull(eq.GetEquipped(GearData.GearSlot.Legs), ref total);
                AddGearWeightIfNotNull(eq.GetEquipped(GearData.GearSlot.Boots), ref total);
            }
        }

        float old = currentWeight;
        currentWeight = Mathf.Max(0f, total);

        if (!Mathf.Approximately(old, currentWeight))
            OnWeightChanged?.Invoke(Weight);
    }

    private void AddGearWeightIfNotNull(GearData gear, ref float total)
    {
        if (gear != null)
            total += gear.weight;
    }

    public float HungerRateMultiplier
    {
        get => hungerRateMultiplier;
        set => hungerRateMultiplier = Mathf.Max(0f, value);
    }

    public float HydrationRateMultiplier
    {
        get => hydrationRateMultiplier;
        set => hydrationRateMultiplier = Mathf.Max(0f, value);
    }

    public float EnergyRateMultiplier
    {
        get => energyRateMultiplier;
        set => energyRateMultiplier = Mathf.Max(0f, value);
    }

    public float HealthRegenMultiplier
    {
        get => healthRegenMultiplier;
        set => healthRegenMultiplier = Mathf.Max(0f, value);
    }

    public float TemperatureRateMultiplier
    {
        get => temperatureRateMultiplier;
        set => temperatureRateMultiplier = Mathf.Max(0f, value);
    }

    private void TickNaturalStats(float dt)
    {
        if (hungerDrainPerSecond > 0f && currentHunger > 0f)
        {
            float hungerDelta = -hungerDrainPerSecond * hungerRateMultiplier * dt;
            ChangeHunger(hungerDelta);
        }

        if (hydrationDrainPerSecond > 0f && currentHydration > 0f)
        {
            float hydrationDelta = -hydrationDrainPerSecond * hydrationRateMultiplier * dt;
            ChangeHydration(hydrationDelta);
        }

        if (currentEnergy < energyCap && energyRegenPerSecond > 0f)
        {
            float energyDelta = energyRegenPerSecond * energyRateMultiplier * dt;
            ChangeEnergy(energyDelta);
        }

        if (energyDrainPerSecond > 0f && currentEnergy > 0f)
        {
            float energyDrain = -energyDrainPerSecond * energyRateMultiplier * dt;
            ChangeEnergy(energyDrain);
        }

        if (healthRegenPerSecond > 0f && currentHealth < healthCap)
        {
            if (currentHunger > hungerCap * 0.5f && currentHydration > hydrationCap * 0.5f)
            {
                float hpDelta = healthRegenPerSecond * healthRegenMultiplier * dt;
                ChangeHealth(hpDelta);
            }
        }

        if (healthDegenerationPerSecond > 0f)
        {
            float hpLose = -healthDegenerationPerSecond * dt;
            ChangeHealth(hpLose);
        }

        if (!Mathf.Approximately(temperature, normalTemperature))
        {
            float dir = Mathf.Sign(normalTemperature - temperature);
            float tempDelta = dir * temperatureChangeTowardsNormalPerSecond * temperatureRateMultiplier * dt;
            ChangeTemperature(tempDelta);
        }
    }

    public PlayerStatsSave Capture()
    {
        return new PlayerStatsSave
        {
            health = Health,
            hunger = Hunger,
            hydration = Hydration,
            energy = Energy,
            temperature = Temperature
        };
    }

    public void Restore(PlayerStatsSave s)
    {
        ChangeHealth(s.health - Health);
        ChangeHunger(s.hunger - Hunger);
        ChangeHydration(s.hydration - Hydration);
        ChangeEnergy(s.energy - Energy);
        ChangeTemperature(s.temperature - Temperature);
    }
}
