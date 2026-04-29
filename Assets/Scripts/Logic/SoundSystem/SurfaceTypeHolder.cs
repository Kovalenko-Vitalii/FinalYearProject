using UnityEngine;

// This class is responsible for setting surface type to models on scene
// used for footstep and particle detection
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
