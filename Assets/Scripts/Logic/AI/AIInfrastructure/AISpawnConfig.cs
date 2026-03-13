using UnityEngine;

[CreateAssetMenu(menuName = "AI/AI Spawn Config")]
// This class represents static data about creature population
public class AISpawnConfig : ScriptableObject
{
    public string creatureId; // what creature
    public GameObject prefab; // its prefab

    [Header("Population")]
    public int maxAliveCount = 5; // how many on map
    public float respawnDelay = 60f; // how fast respawn new ones
    public int initialSpawnCount = 3; // how many spawn initially
}