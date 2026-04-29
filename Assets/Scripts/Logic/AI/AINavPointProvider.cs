using UnityEngine;
using UnityEngine.AI;

// This class responsible for selecting random point in selected radius
// It can be used in any AI creature
public class AINavPointProvider : MonoBehaviour
{
    public Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 point = center + new Vector3(
                Random.Range(-radius, radius),
                0f,
                Random.Range(-radius, radius));

            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }

        return center;
    }
}