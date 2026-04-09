using UnityEngine;

[CreateAssetMenu(menuName = "AI/AI Configs/PredatorConfig")]
public class MonsterAIConfig : PassiveCreatureConfig
{
    [Header("Hearing")]
    public float hearingRadius = 14f;

    [Header("Vision")]
    public float visionRange = 7f;
    [Range(0f, 360f)] public float viewAngle = 110f;
    public float eyeHeight = 1.6f;
    public LayerMask visionBlockers = ~0;

    [Header("Chase")]
    public float chaseSpeed = 4.5f;
    public float chaseRepathInterval = 0.15f;
    public float lostSightGraceTime = 1.2f;

    [Header("Search")]
    public float searchDuration = 4f;
    public float searchRadius = 5f;
    public float searchRepathInterval = 0.8f;

    [Header("Attack")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1.0f;
    public int attackDamage = 1;
    public float attackLoseRange = 2.4f;
}
