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
        Debug.Log("-1");
        if (!spawn) return _player;
        Debug.Log("-2");
        if (_player == null)
            _player = Instantiate(playerPrefab);
        Debug.Log("-3");
        var cc = _player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        Debug.Log("-4");
        _player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        if (cc) cc.enabled = true;
        Debug.Log("-5");
        return _player;
    }

    public void Despawn()
    {
        if (_player != null) Destroy(_player);
        _player = null;
    }
}
