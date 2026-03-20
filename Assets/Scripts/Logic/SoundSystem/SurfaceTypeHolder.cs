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
    Flesh
}

public enum ImpactKind
{
    Default,
    Bullet,
    Melee
}

public class SurfaceTypeHolder : MonoBehaviour
{
    public SurfaceType type = SurfaceType.Default;
}
