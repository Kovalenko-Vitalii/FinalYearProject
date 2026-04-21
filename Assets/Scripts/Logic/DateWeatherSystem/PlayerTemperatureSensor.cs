using System.Collections.Generic;
using UnityEngine;

public class PlayerTemperatureSensor : MonoBehaviour
{
    private readonly List<TemperatureZone> zones = new();

    private void OnEnable() => PlayerStatManager.Instance?.BindTemperatureSensor(this);
    private void Start() => PlayerStatManager.Instance?.BindTemperatureSensor(this);
    private void OnDisable() => PlayerStatManager.Instance?.UnbindTemperatureSensor(this);

    public float GetTemperatureDeltaPerSecond()
    {
        float total = 0f;

        for (int i = zones.Count - 1; i >= 0; i--)
        {
            var zone = zones[i];
            if (zone == null)
            {
                zones.RemoveAt(i);
                continue;
            }

            total += zone.TemperatureDeltaPerSecond;
        }

        return total;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out TemperatureZone zone)) return;
        if (zones.Contains(zone)) return;

        zones.Add(zone);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out TemperatureZone zone)) return;
        zones.Remove(zone);
    }
}