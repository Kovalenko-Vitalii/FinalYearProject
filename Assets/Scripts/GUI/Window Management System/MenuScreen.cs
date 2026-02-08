using UnityEngine;

public class MenuScreen : MonoBehaviour, IModalScreen
{
    [SerializeField] private bool blocksGameplay = true;
    [SerializeField] private GameObject root;

    public GameObject Root => root != null ? root : gameObject;
    public bool BlocksGameplay => blocksGameplay;

    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    public AudioClip OpenSound => openSound;
    public AudioClip CloseSound => closeSound;

    public virtual void OnOpen() { }
    public virtual void OnClose() { }
}
