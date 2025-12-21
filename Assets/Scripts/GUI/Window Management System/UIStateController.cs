using UnityEngine;

public class UIStateController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject gameplayUiRoot;
    [SerializeField] private CanvasSwitcher canvasSwitcher;

    public void EnterMainMenu()
    {
        if (canvasSwitcher != null)
            canvasSwitcher.ClearAll();

        if (mainMenuRoot) mainMenuRoot.SetActive(true);
        if (gameplayUiRoot) gameplayUiRoot.SetActive(false);
    }

    public void EnterGameplay()
    {
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (gameplayUiRoot) gameplayUiRoot.SetActive(true);
    }
}
