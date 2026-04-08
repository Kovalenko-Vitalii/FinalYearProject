using UnityEngine;

public class AIVisionSensor : MonoBehaviour
{
    [SerializeField] private Transform eyePoint;

    public bool CanSee(
        Transform target,
        float range,
        float angleDeg,
        float eyeHeight,
        LayerMask blockers)
    {
        if (target == null)
            return false;

        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * eyeHeight;

        Vector3 targetPoint = target.position + Vector3.up * 1f;
        Vector3 toTarget = targetPoint - origin;

        if (toTarget.sqrMagnitude > range * range)
            return false;

        Vector3 flatDirection = Vector3.ProjectOnPlane(toTarget, Vector3.up);
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.Angle(transform.forward, flatDirection.normalized);
            if (angle > angleDeg * 0.5f)
                return false;
        }

        float distance = toTarget.magnitude;

        if (Physics.Raycast(
                origin,
                toTarget.normalized,
                out RaycastHit hit,
                distance,
                blockers,
                QueryTriggerInteraction.Ignore))
        {
            if (!hit.transform.IsChildOf(target))
                return false;
        }

        return true;
    }
}