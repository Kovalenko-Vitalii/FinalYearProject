using UnityEngine;

public class UIInputRouter : MonoBehaviour
{
    [SerializeField] private CanvasSwitcher ui;

    private void Update()
    {
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
}
