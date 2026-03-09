using UnityEngine;
using UnityEngine.AI;

// This class represents contex of AI creature
public class AIContext : MonoBehaviour
{
    [field: SerializeField] public NavMeshAgent Agent { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public AIHealth Health { get; private set; }
    [field: SerializeField] public AIThreatMemory ThreatMemory { get; private set; }

    private void Reset()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        Health = GetComponent<AIHealth>();
        ThreatMemory = GetComponent<AIThreatMemory>();
    }
}

// This interface is applied to AI creatures that can take damage
public interface IDamageable
{
    void TakeDamage(int damage, Vector3 hitPoint, GameObject source);
}