using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerEnterActivator : MonoBehaviour, ISaveable
{
    [SerializeField] private string id;
    [SerializeField] private InteractExecutor executor;
    [SerializeField] private bool onlyPlayer = true;
    [SerializeField] private bool onlyOnce = true;

    [Header("Execution Policy")]
    [SerializeField] private ExecutePolicy policy = ExecutePolicy.IgnoreRequirements | ExecutePolicy.IgnoreCosts;

    private bool used;

    public string SaveId => id;

    private void Reset()
    {
#if UNITY_EDITOR
        SaveIdUtil.EnsureId(ref id, this);
#else
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
#endif
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        executor = GetComponent<InteractExecutor>();
    }

#if UNITY_EDITOR
    private void OnValidate() => SaveIdUtil.EnsureId(ref id, this);
#endif

    private void Awake()
    {
        if (executor == null)
            executor = GetComponent<InteractExecutor>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && used) return;
        if (executor == null) return;

        if (onlyPlayer && !other.CompareTag("Player")) return;

        var interactor = other.GetComponentInParent<PlayerInteractor>();
        var ctx = new InteractContext(executor, other.gameObject, interactor);

        if (executor.Execute(ctx, policy) && onlyOnce)
            used = true;
    }

    // ---------------- ISaveable ----------------
    public object CaptureState()
    {
        return new TriggerEnterActivatorState
        {
            used = used
        };
    }

    public void RestoreState(object state)
    {
        if (state is not TriggerEnterActivatorState s) return;
        used = s.used;
    }

    public void ResetToDefaultState()
    {

    }
}

[Serializable]
public struct TriggerEnterActivatorState
{
    public bool used;
}