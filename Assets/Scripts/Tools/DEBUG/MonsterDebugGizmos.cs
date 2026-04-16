using UnityEngine;

[RequireComponent(typeof(MonsterBrain))]
public class MonsterDebugGizmos : MonoBehaviour
{
    [SerializeField] private bool drawAlways;
    [SerializeField] private bool drawHearing = true;
    [SerializeField] private bool drawVision = true;
    [SerializeField] private bool drawMemory = true;

    private MonsterBrain brain;

    private void OnDrawGizmos()
    {
        if (drawAlways)
            DrawGizmosInternal();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmosInternal();
    }

    private void DrawGizmosInternal()
    {
        if (brain == null)
            brain = GetComponent<MonsterBrain>();

        if (brain == null)
            return;

        MonsterAIConfig config = brain.Config as MonsterAIConfig;
        AIContext context = brain.Context;

        if (config == null || context == null)
            return;

        Vector3 monsterPos = transform.position;
        Vector3 eyePos = monsterPos + Vector3.up * config.eyeHeight;

        if (drawHearing)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
            Gizmos.DrawWireSphere(monsterPos, config.hearingRadius);
        }

        if (drawVision)
        {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 1f);

            Vector3 leftDir = DirectionFromAngle(-config.viewAngle * 0.5f);
            Vector3 rightDir = DirectionFromAngle(config.viewAngle * 0.5f);
            Vector3 forwardDir = transform.forward;

            Gizmos.DrawLine(eyePos, eyePos + forwardDir * config.visionRange);
            Gizmos.DrawLine(eyePos, eyePos + leftDir * config.visionRange);
            Gizmos.DrawLine(eyePos, eyePos + rightDir * config.visionRange);

            Gizmos.DrawWireSphere(eyePos, 0.12f);
        }

        if (drawMemory && context.MonsterMemory != null)
        {
            var memory = context.MonsterMemory;

            if (memory.HasHeardNoise)
            {
                Gizmos.color = new Color(0.25f, 0.6f, 1f, 1f);
                Gizmos.DrawSphere(memory.LastHeardPosition, 0.2f);
                Gizmos.DrawLine(monsterPos, memory.LastHeardPosition);

                Gizmos.color = new Color(0.25f, 0.6f, 1f, 0.8f);
                Gizmos.DrawWireSphere(memory.LastHeardPosition, config.searchRadius);

                if (memory.LastHeardRadius > 0f)
                {
                    Gizmos.color = new Color(0.25f, 0.9f, 1f, 0.5f);
                    Gizmos.DrawWireSphere(memory.LastHeardPosition, memory.LastHeardRadius);
                }
            }

            if (memory.HasSeenTarget)
            {
                Gizmos.color = new Color(1f, 0.25f, 0.25f, 1f);
                Gizmos.DrawSphere(memory.LastKnownTargetPosition, 0.22f);
                Gizmos.DrawLine(monsterPos, memory.LastKnownTargetPosition);
            }
        }
    }

    private Vector3 DirectionFromAngle(float localAngle)
    {
        float angle = transform.eulerAngles.y + localAngle;
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
    }
}