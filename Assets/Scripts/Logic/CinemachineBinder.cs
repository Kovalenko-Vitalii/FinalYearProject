using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

// This class binds cinemachine to player`s head postion
public class CinemachineBinder : MonoBehaviour
{
    string TAG = "CineBind";
    [SerializeField] private string headPath = "HeadPosition";

    public void BindForActivePlayer()
    {
        var player = PlayerSpawner.Instance?.Player;
        if (player == null)
        {
            GameLog.Error(TAG, "Player is null, skipping bind");
            return;
        }

        var follow = player.transform.Find(headPath);
        if (follow == null)
        {
            GameLog.Error(TAG, $"'{headPath}' not found under player '{player.name}'");
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
            GameLog.Error(TAG, "No CinemachineCamera found in ACTIVE scene");
    }
}
