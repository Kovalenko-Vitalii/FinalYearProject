using UnityEngine;

[CreateAssetMenu(menuName = "Game/Location Transition")]
public class LocationTransition : ScriptableObject
{
    public string sceneName;
    public string spawnId = "Start";
}
