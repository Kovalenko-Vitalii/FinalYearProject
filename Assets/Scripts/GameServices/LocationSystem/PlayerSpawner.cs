using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    string TAG = "PlayerSpawner";
    public static PlayerSpawner Instance { get; private set; }

    // Prefab of player bean
    [SerializeField] private GameObject playerPrefab;

    // Reference for spawned active player
    private GameObject _player;

    private void Awake()
    {
        GameLog.Log(TAG, $"Awake id={GetInstanceID()} hasPrefab={(playerPrefab ? playerPrefab.name : "NULL")}");

        if (Instance != null && Instance != this) 
        {
            GameLog.Warning(TAG, "Duplicate -> destroy");
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }

    // _player that can be accesible
    public GameObject Player => _player;

    // Method used for spawning a player to spawnpoint
    public GameObject SpawnOrMoveTo(Transform spawn)
    {
        // Checking if there is everything needed for action
        if (!spawn)
        {
            GameLog.Warning(TAG, "SpawnOrMoveTo called with NULL spawn transform");
            return _player;
        }

        if (_player == null)
            _player = Instantiate(playerPrefab);

        // Disabling player`s character controller
        var cc = _player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
 
        // Teleporting player
        _player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        Physics.SyncTransforms();

        // Unparalise player
        if (cc) cc.enabled = true;

        GameLog.Log(TAG, $"SpawnOrMoveTo(Transform) -> pos={spawn.position} spawn='{spawn.name}'");
        return _player;
    }

    public GameObject SpawnOrMoveTo(Vector3 position, Quaternion rotation)
    {
        if (_player == null)
            _player = Instantiate(playerPrefab);

        var cc = _player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        _player.transform.SetPositionAndRotation(position, rotation);

        if (cc) cc.enabled = true;

        GameLog.Log(TAG, $"SpawnOrMoveTo(Vector3) -> pos={position} rotY={rotation.eulerAngles.y:0.0}");
        return _player;
    }


    // Despawn player bean
    public void Despawn()
    {
        if (_player == null)
        {
            GameLog.Log(TAG, "Despawn: player already null");
            return;
        }

        GameLog.Log(TAG, $"Despawn destroying '{_player.name}' scene='{_player.scene.name}'");
        Destroy(_player);
        _player = null;
    }

    public GameObject EnsureSpawned()
    {
        if (_player == null)
        {
            if (!playerPrefab)
            {
                GameLog.Error(TAG, "EnsureSpawned failed: playerPrefab is NULL");
                return null;
            }
            _player = Instantiate(playerPrefab);
            GameLog.Log(TAG, $"Player instantiated name='{_player.name}' scene='{_player.scene.name}'");
        }
        else
            GameLog.Log(TAG, $"EnsureSpawned: already exists name='{_player.name}' scene='{_player.scene.name}'");

        return _player;
    }

}
