using UnityEngine;

[CreateAssetMenu(menuName = "AI/Configs/Rabbit")]
public class RabbitConfig : ScriptableObject
{
    [Header("Health")]
    public int maxHp = 1;

    [Header("Idle")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    [Header("Wander")]
    public float wanderRadius = 8f;
    public float walkSpeed = 2f;

    [Header("Flee")]
    public float runSpeed = 5f;
    public float fleeDuration = 3f;
    public float fleeDistance = 10f;
    public float fleeRepathInterval = 0.25f;
}