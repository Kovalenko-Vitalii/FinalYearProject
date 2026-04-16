using UnityEngine;

public class UIInputRouter : MonoBehaviour
{
    [SerializeField] private CanvasSwitcher ui;

    private void Update()
    {
        if (!CanUseGameplayHotkeys())
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ui.AnyOpen)
            {
                ui.CloseTop();
            }
            else
            {
                ui.TryOpenEscapeMain();
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (ui.IsOpen(ui.TabMenu))
            {
                ui.CloseTop();
            }
            else if (!ui.AnyOpen)
            {
                ui.TryOpenTabMain();
            }
            else
            {

            }
        }
    }

    public void CloseCurrent()
    {
        if (ui.AnyOpen)
        {
            ui.CloseTop();
        }
    }

    private bool CanUseGameplayHotkeys()
    {
        var orch = GameplayOrchestrator.Instance;
        return orch != null && orch.State == GameplayOrchestrator.GameState.Gameplay;
    }

}
