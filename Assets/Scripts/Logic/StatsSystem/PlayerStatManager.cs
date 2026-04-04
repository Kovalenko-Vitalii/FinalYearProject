using UnityEngine;
using System;
using static GameplayOrchestrator;

public class PlayerStatManager : MonoBehaviour, ISaveable
{
    public static PlayerStatManager Instance { get; private set; }

    public string SaveId => "PLAYER_STATS";

    // List of stats, include current, maximum
    [Header("Health settings")]
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float healthCap = 100f;

    [Header("Hunger settings")]
    [SerializeField] private float currentHunger = 100f;
    [SerializeField] private float hungerCap = 100f;

    [Header("Hydration settings")]
    [SerializeField] private float currentHydration = 100f;
    [SerializeField] private float hydrationCap = 100f;

    [Header("Temperature settings")]
    [SerializeField] private float temperature = 36.6f;
    [SerializeField] private float minTemperature = 30f;
    [SerializeField] private float maxTemperature = 42f;
    [SerializeField] private float normalTemperature = 36.6f;


    [Header("Energy settings")]
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float energyCap = 100f;

    [Header("Stamina settings")]
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float staminaCap = 100f;



    [Header("Weight settings")]
    [SerializeField] private float baseWeight = 0f;
    [SerializeField] private float currentWeight = 0f;
    [SerializeField] private float maxCarryWeight = 35f;

    [Header("Resistances")]
    [SerializeField] private float temperatureResist = 0;
    [SerializeField] private float damageResist = 0;

    [Header("Temperature Simulation")]
    [SerializeField] private PlayerTemperatureSensor temperatureSensor;
    [SerializeField] private float comfortableAmbientTemperature = 22f;
    [SerializeField] private float bodyTempPerAmbientDegree = 0.08f;
    [SerializeField] private float baseTemperatureChangePerSecond = 0.35f;
    [SerializeField] private float sprintBodyHeatBonus = 0.25f;
    [SerializeField] private float resistForFullProtection = 100f;

    // Regen parameters
    [Header("Natural Change Rates (per second)")]
    [SerializeField] private float healthRegenPerSecond = 0f; 
    [SerializeField] private float hungerDrainPerSecond = 0.2f;
    [SerializeField] private float hydrationDrainPerSecond = 0.3f;
    [SerializeField] private float energyDrainPerSecond = 1f;
    [SerializeField] private float staminaRegenPerSecond = 18f;
    [SerializeField] private float minStaminaToStartSprint = 10f;

    private float _staminaRegenBlockedUntil;

    [Header("Runtime Debug (read only)")]
    [SerializeField] private StatusEffectsSnapshot dbgSnapshot;

    // Events
    public event Action<float> OnHealthChanged;
    public event Action<float> OnHungerChanged;
    public event Action<float> OnHydrationChanged;
    public event Action<float> OnTemperatureChanged;
    public event Action<float> OnEnergyChanged;
    public event Action<float> OnTemperatureResistChanged;
    public event Action<float> OnDamageResistChanged;
    public event Action<float> OnWeightChanged;
    public event Action<float> OnStaminaChanged;


    public event Action OnDied;
    public bool IsDead { get; private set; }

    public StatusEffectsSnapshot CurrentSnapshot { get; private set; } = StatusEffectsSnapshot.Default;


    public float Health => currentHealth;
    public float Hunger => currentHunger;
    public float Hydration => currentHydration;
    public float Temperature => temperature;
    public float Energy => currentEnergy;

    public float CurrentWeight => currentWeight + baseWeight;
    public float MaxCarryWeight => maxCarryWeight;

    public float HealthMax => healthCap;
    public float HungerMax => hungerCap;
    public float HydrationMax => hydrationCap;
    public float EnergyMax => energyCap;
    public float TemperatureMin => minTemperature;
    public float TemperatureMax => maxTemperature;

    public float Stamina => currentStamina;
    public float StaminaMax => staminaCap;
    public float MinStaminaToStartSprint => minStaminaToStartSprint;

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
        // This subscription should be moved out
        var invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            invMgr.OnPlayerInventoryChanged += HandleInventoryChanged;
            invMgr.OnEquipmentChanged += HandleEquipmentChanged;
        }

        OnStaminaChanged?.Invoke(currentStamina);
        OnHealthChanged?.Invoke(currentHealth);
        OnHungerChanged?.Invoke(currentHunger);
        OnHydrationChanged?.Invoke(currentHydration);
        OnTemperatureChanged?.Invoke(temperature);
        OnEnergyChanged?.Invoke(currentEnergy);
        OnTemperatureResistChanged?.Invoke(temperatureResist);
        OnDamageResistChanged?.Invoke(damageResist);

        RecalculateWeight();
        RecalculateResistances();
    }

    private void OnDestroy()
    {
        var invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            invMgr.OnPlayerInventoryChanged -= HandleInventoryChanged;
            invMgr.OnEquipmentChanged -= HandleEquipmentChanged;
        }
    }
    // Applying consumable
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

    // Changing stats according to cap rules and sends event if changed value is big enough (for optimisation)
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

    // Stamina 
    public void ChangeStamina(float amount)
    {
        float old = currentStamina;
        currentStamina = Mathf.Clamp(currentStamina + amount, 0f, staminaCap);
        if (!Mathf.Approximately(old, currentStamina))
            OnStaminaChanged?.Invoke(currentStamina);
    }

    public bool HasStamina(float amount) => currentStamina >= amount;

    public bool CanStartSprint()
    {
        return currentStamina >= minStaminaToStartSprint;
    }

    public bool CanKeepSprinting()
    {
        return currentStamina > 0f;
    }


    public bool TryConsumeStamina(float amount, float regenDelay)
    {
        if (amount <= 0f) return true;
        if (currentStamina < amount) return false;

        ChangeStamina(-amount);
        _staminaRegenBlockedUntil = Time.time + Mathf.Max(0f, regenDelay);
        return true;
    }

    public void TickStamina(float dt, in StatusEffectsSnapshot s, bool isSprinting)
    {
        if (isSprinting) return;
        if (Time.time < _staminaRegenBlockedUntil) return;
        if (staminaRegenPerSecond <= 0f) return;
        if (currentStamina >= staminaCap) return;

        ChangeStamina(staminaRegenPerSecond * s.StaminaRegenModifier * dt);
    }


    // Applying gear
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

    // Recalculating weight when changed inventory or gear
    private void HandleInventoryChanged()
    {
        RecalculateWeight();
        RecalculateResistances();
    }

    private void HandleEquipmentChanged()
    {
        RecalculateWeight();
        RecalculateResistances();
    }

    // Recalculating weight based on inventory and gear list
    public void RecalculateWeight()
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

            var eq = invMgr.playerEquippedItems;
            if (eq != null)
            {
                foreach (var kv in eq.Slots)
                    AddEquippedItemWeight(kv.Value, ref total);
            }
        }

        float old = currentWeight;
        currentWeight = Mathf.Max(0f, total);

        if (!Mathf.Approximately(old, currentWeight))
            OnWeightChanged?.Invoke(CurrentWeight);
    }
    private void AddEquippedItemWeight(InventoryItem item, ref float total)
    {
        if (item?.data != null)
            total += item.data.weight * item.amount;
    }

    public void RecalculateResistances()
    {
        float newTemperatureResist = 0f;
        float newDamageResist = 0f;

        var invMgr = InventoryManager.Instance;
        var eq = invMgr != null ? invMgr.playerEquippedItems : null;

        if (eq != null)
        {
            foreach (var kv in eq.Slots)
                AddGearResist(kv.Value, ref newTemperatureResist, ref newDamageResist);
        }

        if (!Mathf.Approximately(temperatureResist, newTemperatureResist))
        {
            temperatureResist = newTemperatureResist;
            OnTemperatureResistChanged?.Invoke(temperatureResist);
        }

        if (!Mathf.Approximately(damageResist, newDamageResist))
        {
            damageResist = newDamageResist;
            OnDamageResistChanged?.Invoke(damageResist);
        }
    }

    private void AddGearResist(InventoryItem item, ref float totalTemperatureResist, ref float totalDamageResist)
    {
        if (item?.data is not GearData gear)
            return;

        totalTemperatureResist += gear.temperatureResist;
        totalDamageResist += gear.damageResist;
    }


    // Ticking stats (I know it should be optimised)
    public void TickNaturalStats(float dt, in StatusEffectsSnapshot s, bool isSprinting)
    {
        // hunger drain
        if (hungerDrainPerSecond > 0f && currentHunger > 0f)
            ChangeHunger(-hungerDrainPerSecond * s.HungerRateModifier * dt);

        // hydration drain
        if (hydrationDrainPerSecond > 0f && currentHydration > 0f)
            ChangeHydration(-hydrationDrainPerSecond * s.HydrationRateModifier * dt);

        // health regen
        if (healthRegenPerSecond > 0f && currentHealth < healthCap)
            ChangeHealth(+healthRegenPerSecond * s.HealthRegenModifier * dt);

        // energy regen
        if (energyDrainPerSecond > 0f && currentEnergy > 0f)
            ChangeEnergy(-energyDrainPerSecond * s.EnergyRateModifier * dt);

        // health decreasing
        if (s.HealthDegenerationPerSecond > 0f)
            ChangeHealth(-s.HealthDegenerationPerSecond * dt);

        // stamina ticking
        TickStamina(dt, s, isSprinting);
    }

    public void Tick(float dt, bool isSprinting)
    {
        TickTemperature(dt, isSprinting);

        var s = StatusEffectsSnapshot.Default;

        // Influence of stats like low water - loosing hp etc.
        StatInfluenceSystem.ApplyFromStats(this, ref s);

        // Influence of stats
        var effects = StatusEffectManager.Instance;
        effects?.ApplyAllTo(ref s);

        // Applying snapshot to stats
        TickNaturalStats(dt, s, isSprinting);

        // death detect
        if (!IsDead && currentHealth <= 0f)
        {
            IsDead = true;
            OnDied?.Invoke();

            if (GameplayOrchestrator.Instance != null)
                GameplayOrchestrator.Instance.EnterDied();
        }

        CurrentSnapshot = s;

        // debug
        dbgSnapshot = s;
    }

    public void BindTemperatureSensor(PlayerTemperatureSensor sensor)
    {
        temperatureSensor = sensor;
    }

    public void UnbindTemperatureSensor(PlayerTemperatureSensor sensor)
    {
        if (temperatureSensor == sensor)
            temperatureSensor = null;
    }

    private void TickTemperature(float dt, bool isSprinting)
    {
        TemperatureContext ctx = temperatureSensor != null
            ? temperatureSensor.GetContext()
            : new TemperatureContext
            {
                AmbientTemperature = 20f
            };

        float protect01 = Mathf.Clamp01(
            temperatureResist / Mathf.Max(1f, resistForFullProtection));

        float effectiveAmbient = Mathf.Lerp(
            ctx.AmbientTemperature,
            comfortableAmbientTemperature,
            protect01);

        float ambientDelta = effectiveAmbient - comfortableAmbientTemperature;
        float targetBodyTemperature = normalTemperature + ambientDelta * bodyTempPerAmbientDegree;

        if (isSprinting)
            targetBodyTemperature += sprintBodyHeatBonus;

        float old = temperature;
        temperature = Mathf.MoveTowards(
            temperature,
            targetBodyTemperature,
            baseTemperatureChangePerSecond * dt);

        temperature = Mathf.Clamp(temperature, minTemperature, maxTemperature);

        if (!Mathf.Approximately(old, temperature))
            OnTemperatureChanged?.Invoke(temperature);
    }

    // For storing and restoring stats
    public object CaptureState()
    {
        return new PlayerStatsSave
        {
            health = Health,
            hunger = Hunger,
            hydration = Hydration,
            energy = Energy,
            stamina = Stamina,
            temperature = Temperature
        };
    }

    public void RestoreState(object state)
    {
        var s = state as PlayerStatsSave;
        if (s == null) return;

        ChangeHealth(s.health - Health);
        ChangeHunger(s.hunger - Hunger);
        ChangeHydration(s.hydration - Hydration);
        ChangeEnergy(s.energy - Energy);
        ChangeTemperature(s.temperature - Temperature);
        ChangeStamina(s.stamina - Stamina);

        IsDead = currentHealth <= 0f;

        RecalculateWeight();
    }

    public void ResetToDefaults()
    {
        currentHealth = healthCap;
        currentHunger = hungerCap;
        currentHydration = hydrationCap;
        currentEnergy = energyCap;
        temperature = normalTemperature;

        OnHealthChanged?.Invoke(currentHealth);
        OnHungerChanged?.Invoke(currentHunger);
        OnHydrationChanged?.Invoke(currentHydration);
        OnEnergyChanged?.Invoke(currentEnergy);
        OnTemperatureChanged?.Invoke(temperature);

        IsDead = false;

        RecalculateWeight();
    }
}

// Saving player stats
[Serializable]
public class PlayerStatsSave
{
    public float health, hunger, hydration, energy, temperature, stamina;
}