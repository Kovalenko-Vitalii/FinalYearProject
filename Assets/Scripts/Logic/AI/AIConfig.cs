using UnityEngine;

[CreateAssetMenu(menuName = "AI/AI Configs/PassiveAnimalConfig")]
public class AIConfig : ScriptableObject
{
    public string creatureId;

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

    [Header("Loot")]
    public ItemData dropItem;
    public int dropMinAmount = 1;
    public int dropMaxAmount = 1;
    [Range(0f, 1f)] public float dropChance = 1f;
}