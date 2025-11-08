using System;
using System.Collections.Generic;

public class ModalStack
{
    private readonly Stack<IModalScreen> _stack = new();

    public event Action<IModalScreen, bool> OnStackChanged;

    public bool AnyOpen => _stack.Count > 0;
    public IModalScreen Top => _stack.Count > 0 ? _stack.Peek() : null;

    public void Push(IModalScreen screen)
    {
        if (screen == null) return;
        if (screen.Root.activeSelf) return;

        screen.Root.SetActive(true);
        screen.OnOpen();
        _stack.Push(screen);
        OnStackChanged?.Invoke(screen, AnyOpen);
    }

    public void PopTop()
    {
        if (_stack.Count == 0) return;
        var top = _stack.Pop();
        top.OnClose();
        top.Root.SetActive(false);
        OnStackChanged?.Invoke(top, AnyOpen);
    }

    public void Remove(IModalScreen screen)
    {
        if (screen == null || _stack.Count == 0) return;

        var temp = new Stack<IModalScreen>();
        bool removed = false;

        while (_stack.Count > 0)
        {
            var s = _stack.Pop();
            if (s == screen && !removed)
            {
                s.OnClose();
                s.Root.SetActive(false);
                removed = true;
                break;
            }
            temp.Push(s);
        }
        while (temp.Count > 0) _stack.Push(temp.Pop());

        if (removed) OnStackChanged?.Invoke(screen, AnyOpen);
    }

    public void Clear()
    {
        while (_stack.Count > 0)
        {
            var s = _stack.Pop();
            s.OnClose();
            s.Root.SetActive(false);
        }
        OnStackChanged?.Invoke(null, AnyOpen);
    }
}
