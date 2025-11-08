using UnityEngine;

public class MenuScreen : MonoBehaviour, IModalScreen
{
    [SerializeField] private bool blocksGameplay = true;
    [SerializeField] private GameObject root;

    public GameObject Root => root != null ? root : gameObject;
    public bool BlocksGameplay => blocksGameplay;

    public void OnOpen() { }
    public void OnClose() { }
}
