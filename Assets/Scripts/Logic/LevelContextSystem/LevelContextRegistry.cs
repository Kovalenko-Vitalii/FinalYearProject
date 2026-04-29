using System;
using UnityEngine;

// This class is storing current levelcontext and providing easy access to objects in it
public class LevelContextRegistry : MonoBehaviour
{
    public static LevelContextRegistry Instance { get; private set; }
    public LevelContext Active { get; private set; }

    public event Action<LevelContext> OnContextChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetActive(LevelContext ctx)
    {
        Active = ctx;
        OnContextChanged?.Invoke(ctx);
    }
}
