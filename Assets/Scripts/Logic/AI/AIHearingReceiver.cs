using System;
using System.Collections.Generic;
using UnityEngine;

public class AIHearingReceiver : MonoBehaviour
{
    public event Action<Vector3> NoiseHeard;

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

    public void Hear(Vector3 noisePosition)
    {
        NoiseHeard?.Invoke(noisePosition);
    }

    public static void BroadcastNoise(Vector3 noisePosition)
    {
        for (int i = ActiveReceivers.Count - 1; i >= 0; i--)
        {
            var receiver = ActiveReceivers[i];

            if (receiver == null)
            {
                ActiveReceivers.RemoveAt(i);
                continue;
            }

            receiver.Hear(noisePosition);
        }
    }
}