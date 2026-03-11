using UnityEngine;

public enum SurfaceType {
    Default,
    Snow, 
    Ice, 
    Concrete, 
    Gravel, 
    Metal, 
    Wood, 
    Dirt, 
    Mud,
    CrackingWood,
}

public class SurfaceSound : MonoBehaviour
{
    public SurfaceType type = SurfaceType.Default;
}
