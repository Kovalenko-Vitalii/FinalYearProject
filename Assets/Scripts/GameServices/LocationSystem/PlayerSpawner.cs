using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    // Prefab of player bean
    [SerializeField] private GameObject playerPrefab;

    // Reference for spawned active player
    private GameObject _player;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // _player that can be accesible
    public GameObject Player => _player;

    // Method used for spawning a player to spawnpoint
    public GameObject SpawnOrMoveTo(Transform spawn)
    {
        // Checking if there is everything needed for action
        if (!spawn) return _player;

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

        // Set player reference
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

        return _player;
    }


    // Despawn player bean
    public void Despawn()
    {
        if (_player != null) Destroy(_player);
        _player = null;
    }
}
