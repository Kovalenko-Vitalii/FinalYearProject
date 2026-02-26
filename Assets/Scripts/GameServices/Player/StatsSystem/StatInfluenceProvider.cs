using UnityEngine;

public class StatInfluenceProvider : MonoBehaviour
{
    public static StatInfluenceProvider Instance { get; private set; }
    public StatInfluenceConfig config;

    private void Awake()
    {
        Instance = this;
    }
}
