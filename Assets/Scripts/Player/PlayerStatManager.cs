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

    [Header("Carried Weight")]
    [SerializeField] private float baseWeight = 0f;        // базовый вес тела/экипы, если хочешь
    [SerializeField] private float currentWeight = 0f;     // динамический вес (инвентарь + шмот)
    [SerializeField] private float maxCarryWeight = 35f;   // лимит переноса (для дебаффов и UI)

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

    // Events
    public event Action<float> OnHealthChanged;
    public event Action<float> OnHungerChanged;
    public event Action<float> OnHydrationChanged;
    public event Action<float> OnTemperatureChanged;
    public event Action<float> OnEnergyChanged;
    public event Action<float> OnTemperatureResistChanged;
    public event Action<float> OnDamageResistChanged;
    public event Action<float> OnWeightChanged;    // <- один параметр: текущий вес (с учётом baseWeight)

    public float Health => currentHealth;
    public float Hunger => currentHunger;
    public float Hydration => currentHydration;
    public float Temperature => temperature;
    public float Energy => currentEnergy;

    public float Weight => currentWeight + baseWeight;
    public float CurrentWeight => Weight;          // для удобства в UI, если хочешь
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
}
