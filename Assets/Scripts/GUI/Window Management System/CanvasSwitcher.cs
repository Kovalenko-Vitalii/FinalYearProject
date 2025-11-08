using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    public static CanvasSwitcher Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private MenuScreen escapeMenu;
    [SerializeField] private MenuScreen tabMenu;

    [SerializeField] private MenuScreen containerScreen;

    public IModalScreen EscapeMenu => (IModalScreen)escapeMenu;
    public IModalScreen TabMenu => (IModalScreen)tabMenu;
    public IModalScreen Container => (IModalScreen)containerScreen;
    public bool AnyOpen => _stack.AnyOpen;

    private readonly ModalStack _stack = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (var s in GetComponentsInChildren<MonoBehaviour>(true))
            if (s is IModalScreen ms) ms.Root.SetActive(false);

        _stack.OnStackChanged += HandleStackChanged;
    }

    private void HandleStackChanged(IModalScreen changed, bool anyOpen)
    {
        bool shouldBlock = _stack.Top != null && _stack.Top.BlocksGameplay;
        PauseManager.Instance.SetPlayerControl(!shouldBlock);
        Cursor.lockState = shouldBlock ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = shouldBlock;
    }

    public void ToggleEscape()
    {
        Toggle(TabMenu);
        Toggle(EscapeMenu);
    }

    public void ToggleTab() => Toggle(TabMenu);

    public void OpenContainer() => _stack.Push(Container);

    public void CloseTop() => _stack.PopTop();

    public void Toggle(IModalScreen screen)
    {
        if (screen.Root.activeSelf) _stack.Remove(screen);
        else _stack.Push(screen);
    }
}
