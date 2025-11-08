using UnityEngine;

public class UIInputRouter : MonoBehaviour
{
    [SerializeField] private CanvasSwitcher ui;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ui.AnyOpen)
                ui.CloseTop();
            else
                ui.ToggleEscape();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            ui.ToggleTab();
    }
}
