using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    private GameObject _player;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GameObject Player => _player;

    public GameObject SpawnOrMoveTo(Transform spawn)
    {

        if (!spawn) return _player;

        if (_player == null)
            _player = Instantiate(playerPrefab);

        var cc = _player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
 
        _player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        if (cc) cc.enabled = true;

        return _player;
    }

    public void Despawn()
    {
        if (_player != null) Destroy(_player);
        _player = null;
    }
}
