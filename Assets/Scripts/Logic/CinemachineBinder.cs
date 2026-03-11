using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class CinemachineBinder : MonoBehaviour
{
    [SerializeField] private string headPath = "HeadPosition";

    public void BindForActivePlayer()
    {
        var player = PlayerSpawner.Instance?.Player;
        if (player == null)
        {
            Debug.LogWarning("[CineBind] Player is null, skipping bind");
            return;
        }

        var follow = player.transform.Find(headPath);
        if (follow == null)
        {
            Debug.LogError($"[CineBind] '{headPath}' not found under player '{player.name}'");
            return;
        }

        var active = SceneManager.GetActiveScene();
        var roots = active.GetRootGameObjects();

        int bound = 0;
        foreach (var r in roots)
        {
            foreach (var vcam in r.GetComponentsInChildren<CinemachineCamera>(true))
            {
                vcam.Follow = follow;
                bound++;
            }
        }

        if (bound == 0)
            Debug.LogError("[CineBind] No CinemachineCamera found in ACTIVE scene");
    }
}
