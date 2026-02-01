using UnityEngine;

public class LevelContext : MonoBehaviour
{
    [SerializeField] private Light directionalLight;

    private void OnEnable()
    {
        LevelContextRegistry.Instance.SetActive(this);
    }

    private void OnDisable()
    {
        if (LevelContextRegistry.Instance.Active == this)
            LevelContextRegistry.Instance.SetActive(null);
    }

    public Light DirectionalLight => directionalLight;
}

