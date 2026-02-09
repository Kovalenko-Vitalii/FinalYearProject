using UnityEngine;

public enum SurfaceType { Default, Concrete, Wood, Grass, Metal, Water }

public class SurfaceSound : MonoBehaviour
{
    public SurfaceType type = SurfaceType.Default;
}
