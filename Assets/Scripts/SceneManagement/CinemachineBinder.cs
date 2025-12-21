using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class CinemachineBinder : MonoBehaviour
{
    [SerializeField] private string playerTag = "PlayerDropOrigin";

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindForActivePlayer();
    }

    public void BindForActivePlayer()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (!player) return;
        Debug.Log("eweeee");
        var follow = player.transform.Find("HeadPosition");

        var vcams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        foreach (var vcam in vcams)
        {
            if (follow) vcam.Follow = follow;
        }
    }
}
