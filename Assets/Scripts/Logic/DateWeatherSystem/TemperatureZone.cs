using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TemperatureZone : MonoBehaviour
{
    [SerializeField] private int priority = 0;

    [Header("Ambient")]
    [SerializeField] private float baseTemperature = 5f;
    [SerializeField] private float runtimeHeatBonus = 0f;

    public int Priority => priority;

    public float CurrentTemperature => baseTemperature + runtimeHeatBonus;

    public void SetHeatBonus(float bonus)
    {
        runtimeHeatBonus = bonus;
    }

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }
}