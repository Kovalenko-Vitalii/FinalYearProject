using UnityEngine;

public class StatusEffectDatabase : MonoBehaviour
{
    [SerializeField] private StatusEffectConfig[] configs;

    private void Awake()
    {
        StatusEffectRules.Init(configs);
    }
}
