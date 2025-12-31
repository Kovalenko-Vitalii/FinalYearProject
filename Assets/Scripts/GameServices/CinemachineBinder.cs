using UnityEngine;
using Unity.Cinemachine;

public class CinemachineBinder : MonoBehaviour
{
    [SerializeField] private string headPath = "HeadPosition";

    // This method is used for finding a player`s position for head and bind cinemachine to it
    public void BindForActivePlayer()
    {
        var spawner = PlayerSpawner.Instance;
        if (spawner == null || spawner.Player == null)
        {
            Debug.LogError("[CineBind] Bind called but PlayerSpawner/Player is null");
            return;
        }

        // Getting a player from spawner and its head position (Empty object)
        var player = spawner.Player;
        var follow = player.transform.Find(headPath);

        if (follow == null)
        {
            Debug.LogError($"[CineBind] HeadPosition '{headPath}' not found under player '{player.name}'");
            return;
        }

        // Finding a cinemachine camera
        var vcams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        if (vcams.Length == 0)
        {
            Debug.LogError("[CineBind] No CinemachineCamera found in scene");
            return;
        }

        // Finally setting up a follow target for camera
        foreach (var vcam in vcams)
            vcam.Follow = follow;

        Debug.Log($"[CineBind] Bound {vcams.Length} cameras to {follow.name}");
    }
}
