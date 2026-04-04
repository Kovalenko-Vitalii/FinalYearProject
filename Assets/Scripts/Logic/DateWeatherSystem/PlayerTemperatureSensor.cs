using System.Collections.Generic;
using UnityEngine;

public struct TemperatureContext
{
    public float AmbientTemperature;
}

public class PlayerTemperatureSensor : MonoBehaviour
{
    private readonly List<TemperatureZone> zones = new();
    private TemperatureZone activeZone;

    private void OnEnable()
    {
        PlayerStatManager.Instance?.BindTemperatureSensor(this);
    }

    private void Start()
    {
        PlayerStatManager.Instance?.BindTemperatureSensor(this);
    }

    private void OnDisable()
    {
        PlayerStatManager.Instance?.UnbindTemperatureSensor(this);
    }

    public TemperatureContext GetContext()
    {
        float ambient = TimeEnviromentManager.Instance != null
            ? TimeEnviromentManager.Instance.CurrentGlobalTemperature
            : 20f;

        if (activeZone != null)
        {
            ambient = activeZone.CurrentTemperature;
        }

        return new TemperatureContext
        {
            AmbientTemperature = ambient
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out TemperatureZone zone)) return;
        if (zones.Contains(zone)) return;

        zones.Add(zone);
        RefreshActiveZone();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out TemperatureZone zone)) return;

        zones.Remove(zone);
        RefreshActiveZone();
    }

    private void RefreshActiveZone()
    {
        activeZone = null;
        int bestPriority = int.MinValue;

        for (int i = 0; i < zones.Count; i++)
        {
            var z = zones[i];
            if (z == null) continue;

            if (z.Priority > bestPriority)
            {
                bestPriority = z.Priority;
                activeZone = z;
            }
        }
    }
}