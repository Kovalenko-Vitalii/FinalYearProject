using UnityEngine;

public class HandsRig : MonoBehaviour
{
    public static HandsRig Instance { get; private set; }
    [field: SerializeField] public Transform HeldItemAnchor { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}