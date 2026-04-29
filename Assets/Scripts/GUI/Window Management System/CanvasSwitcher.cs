using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    public static CanvasSwitcher Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private MenuScreen escapeMenu;
    [SerializeField] private MenuScreen tabMenu;
    [SerializeField] private MenuScreen containerScreen;

    public IModalScreen EscapeMenu => escapeMenu;
    public IModalScreen TabMenu => tabMenu;
    public IModalScreen Container => containerScreen;

    public bool AnyOpen => _stack.AnyOpen;
    public IModalScreen Top => _stack.Top;

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
        {
            if (s is IModalScreen ms)
                ms.Root.SetActive(false);
        }

        _stack.OnStackChanged += HandleStackChanged;
    }

    private void HandleStackChanged(IModalScreen changed, bool anyOpen)
    {
        var orch = GameplayOrchestrator.Instance;
        bool isGameplay = orch != null && orch.State == GameplayOrchestrator.GameState.Gameplay;

        bool shouldBlock = Top != null && Top.BlocksGameplay;

        if (isGameplay)
            PauseManager.Instance?.SetPlayerControl(!shouldBlock);     
        else
            PlayerTickSystem.Instance?.SetEnabled(false);

        bool lockCursor = isGameplay && !shouldBlock;
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }

    public bool IsOpen(IModalScreen screen) => screen.Root.activeSelf;
    public void Close(IModalScreen screen) => _stack.Remove(screen);
    public void CloseTop() => _stack.PopTop();
    public void ClearAll() => _stack.Clear();

    public bool TryOpenMain(IModalScreen screen)
    {
        if (screen == null) return false;

        if (AnyOpen)
            return false;

        _stack.Push(screen);
        return true;
    }

    public bool TryOpenEscapeMain() => TryOpenMain(EscapeMenu);
    public bool TryOpenTabMain() => TryOpenMain(TabMenu);
    public bool TryOpenContainerMain() => TryOpenMain(Container);
}
