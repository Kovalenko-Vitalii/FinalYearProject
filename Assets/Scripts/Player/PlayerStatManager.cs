using UnityEngine;
using System;

public class PlayerStatManager : MonoBehaviour
{
    public static PlayerStatManager Instance { get; private set; }

    [Header("Current Stats")]
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float currentHunger = 100f;
    [SerializeField] private float currentHydration = 100f;
    [SerializeField] private float temperature = 36.6f;
    
    [Header("Stats Limmits")]
    [SerializeField] private float healthCap = 100f;
    [SerializeField] private float hungerCap = 100f;
    [SerializeField] private float hydrationCap = 100f;

    public float CurrentHealth => currentHealth;
    public float HealthCap => healthCap;

    public event Action<float> OnHealthChanged;

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
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, healthCap);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, healthCap);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void UpdateHunger(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, hungerCap);
    }

    public void UpdateHydration(float amount)
    {
        currentHydration = Mathf.Clamp(currentHydration + amount, 0, hydrationCap);
    }
}
