using UnityEngine;

// This class represents AI spawnpoint
// A point that has list of allowed id`s for spawn
// Registers itself automatically in AIManager
public class AISpawnPoint : MonoBehaviour
{
    [SerializeField] private string[] allowedCreatureIds; // !!! <=======================================================

    public Vector3 Position => transform.position;
    public string[] AllowedCreatureIds => allowedCreatureIds; // !!! <=======================================================

    private void OnEnable() => AIManager.Instance?.RegisterAISpawnPoint(this);

    private void OnDisable() => AIManager.Instance?.UnregisterAISpawnPoint(this);
    
       
    // This method checks if this spawnpoint supprots selected creature
    public bool Supports(string creatureId)
    {
        if (allowedCreatureIds == null)
            return false;

        for (int i = 0; i < allowedCreatureIds.Length; i++)
        {
            if (allowedCreatureIds[i] == creatureId)
                return true;
        }

        return false;
    }
}