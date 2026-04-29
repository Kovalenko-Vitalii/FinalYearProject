using UnityEngine;
using UnityEngine.AI;

// This class responsible for moving AI using NavMeshAgent
// Basically it is an overlay on NavMeshAgent
// that provides unified handy methods for navigation

public class AIMover : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    private void Reset() => agent = GetComponent<NavMeshAgent>();
        
    // Setting move speed of navMeshAgent
    public void SetSpeed(float speed)
    {
        if (agent != null && agent.enabled)
            agent.speed = speed;
    }

    // Asking navMeshAgent to go to specified location
    public void MoveTo(Vector3 position)
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = false;
        agent.SetDestination(position);
    }

    // Clearing current destination and stopping movement
    // (maybe I should also set speed to 0 here but not sure)
    public void Stop()
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    // Checking if navMeshAgent reached destination point
    public bool HasReachedDestination()
    {
        if (agent == null || !agent.enabled)
            return true;

        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f;
    }

    // Get velocity
    public float VelocityMagnitude()
    {
        if (agent == null || !agent.enabled)
            return 0f;

        return agent.velocity.magnitude;
    }

    // Disabling movement 
    public void Disable()
    {
        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            agent.enabled = false;
        }
    }

    public void SetStoppingDistance(float distance)
    {
        if (agent != null && agent.enabled)
            agent.stoppingDistance = distance;
    }

    public void FaceTowards(Vector3 point, float rotateSpeed = 720f)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }
}