using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private string id = "Start";
    public string Id => id;

    private void OnEnable()
    {
        if (SpawnPointRegistry.Instance)
            SpawnPointRegistry.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (SpawnPointRegistry.Instance)
            SpawnPointRegistry.Instance.Unregister(this);
    }
}
