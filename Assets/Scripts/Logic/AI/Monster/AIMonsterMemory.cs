using UnityEngine;

public class AIMonsterMemory : MonoBehaviour
{
    public Transform CurrentTarget { get; private set; }

    public Vector3 LastKnownTargetPosition { get; private set; }
    public Vector3 LastHeardPosition { get; private set; }

    public float LastSeenTime { get; private set; } = float.NegativeInfinity;
    public float LastHeardTime { get; private set; } = float.NegativeInfinity;
    public float LastHeardRadius { get; private set; }

    public bool HasSeenTarget => LastSeenTime > float.NegativeInfinity;
    public bool HasHeardNoise => LastHeardTime > float.NegativeInfinity;

    public void RememberSeenTarget(Transform target)
    {
        if (target == null)
            return;

        CurrentTarget = target;
        LastKnownTargetPosition = target.position;
        LastSeenTime = Time.time;
    }

    public void RememberNoise(Vector3 position, float radius)
    {
        LastHeardPosition = position;
        LastHeardTime = Time.time;
        LastHeardRadius = radius;
    }

    public bool HasSeenRecently(float seconds)
    {
        return Time.time - LastSeenTime <= seconds;
    }

    public void ClearAll()
    {
        CurrentTarget = null;
        LastKnownTargetPosition = Vector3.zero;
        LastHeardPosition = Vector3.zero;
        LastSeenTime = float.NegativeInfinity;
        LastHeardTime = float.NegativeInfinity;
        LastHeardRadius = 0f;
    }
}