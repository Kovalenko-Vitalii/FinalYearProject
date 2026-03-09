using UnityEngine;

// This class responsible for storing threat object
// It can be used in any AI creature
public class AIThreatMemory : MonoBehaviour
{
    public GameObject LastThreatSource { get; private set; }
    public Vector3 LastThreatPosition { get; private set; }

    public void RememberThreat(GameObject source, Vector3 position)
    {
        LastThreatSource = source;
        LastThreatPosition = position;
    }

    public void Clear()
    {
        LastThreatSource = null;
        LastThreatPosition = Vector3.zero;
    }
}