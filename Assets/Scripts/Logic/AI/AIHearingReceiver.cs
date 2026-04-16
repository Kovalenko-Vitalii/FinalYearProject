using System;
using System.Collections.Generic;
using UnityEngine;

public static class AINoiseRanges
{
    public const float WalkStep = 10f;

    public const float MinorSound = 4f;

    public const float DoorSound = 12f;

    public const float Gunshot = 80f;
}

public readonly struct AINoiseEvent
{
    public readonly Vector3 Position;
    public readonly float Radius;

    public AINoiseEvent(Vector3 position, float radius)
    {
        Position = position;
        Radius = radius;
    }
}

public class AIHearingReceiver : MonoBehaviour
{
    public event Action<AINoiseEvent> NoiseHeard;

    private static readonly List<AIHearingReceiver> ActiveReceivers = new();

    private void OnEnable()
    {
        if (!ActiveReceivers.Contains(this))
            ActiveReceivers.Add(this);
    }

    private void OnDisable()
    {
        ActiveReceivers.Remove(this);
    }

    public void Hear(AINoiseEvent noise)
    {
        NoiseHeard?.Invoke(noise);
    }

    public static void BroadcastNoise(Vector3 noisePosition, float radius)
    {
        AINoiseEvent noise = new AINoiseEvent(noisePosition, radius);

        for (int i = ActiveReceivers.Count - 1; i >= 0; i--)
        {
            AIHearingReceiver receiver = ActiveReceivers[i];

            if (receiver == null)
            {
                ActiveReceivers.RemoveAt(i);
                continue;
            }

            receiver.Hear(noise);
        }
    }
}