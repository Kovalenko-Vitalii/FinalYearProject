using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TemperatureZone : MonoBehaviour
{
    [Header("Temperature")]
    [SerializeField] private float temperatureDeltaPerSecond = 0.25f;

    public float TemperatureDeltaPerSecond => temperatureDeltaPerSecond;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }
}