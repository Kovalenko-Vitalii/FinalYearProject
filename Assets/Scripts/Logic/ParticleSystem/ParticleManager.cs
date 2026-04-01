using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ParticleSystem PlayOneShot(
        ParticleSystem prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        if (prefab == null)
            return null;

        ParticleSystem ps = Instantiate(prefab, position, rotation, parent);
        ps.Play(true);

        return ps;
    }
}