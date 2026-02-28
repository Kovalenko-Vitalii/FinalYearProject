using UnityEngine;

public class UIStateController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject gameplayUiRoot;
    [SerializeField] private CanvasSwitcher canvasSwitcher;
    [SerializeField] private GameObject diedRoot;

    public void EnterDied()
    {
        canvasSwitcher?.ClearAll();

        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (gameplayUiRoot) gameplayUiRoot.SetActive(false);
        if (diedRoot) diedRoot.SetActive(true);
    }

    public void EnterMainMenu()
    {
        if (canvasSwitcher != null)
            canvasSwitcher.ClearAll();

        if (mainMenuRoot) mainMenuRoot.SetActive(true);
        if (gameplayUiRoot) gameplayUiRoot.SetActive(false);
        if (diedRoot) diedRoot.SetActive(false);
    }

    public void EnterGameplay()
    {
        if (canvasSwitcher != null)
            canvasSwitcher.ClearAll();

        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (gameplayUiRoot) gameplayUiRoot.SetActive(true);
        if (diedRoot) diedRoot.SetActive(false);
    }
}
