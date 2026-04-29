using UnityEngine;

// This class is responsible for spawning any kind of particle on the 
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
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        if (prefab == null)
            return null;

        GameObject instance = Instantiate(prefab, position, rotation, parent);
        ParticleSystem ps = instance.GetComponent<ParticleSystem>();

        if (ps == null)
        {
            Debug.LogError($"Prefab '{prefab.name}' has no ParticleSystem component.");
            Destroy(instance);
            return null;
        }

        ps.Play();
        Destroy(instance, ps.main.duration + ps.main.startLifetime.constantMax);
        return ps;
    }
}