using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LocationTransitionTrigger : MonoBehaviour
{
    [SerializeField] private LocationTransition transition;
    [SerializeField] private bool oneShot = true;

    private bool _used;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_used && oneShot) return;
        if (!transition) return;

        var player = PlayerSpawner.Instance ? PlayerSpawner.Instance.Player : null;
        if (!player) return;

        if (other.gameObject != player && other.transform.root.gameObject != player)
            return;

        _used = true;
        GameplayOrchestrator.Instance.LoadLocation(transition.sceneName, transition.spawnId);
    }
}
