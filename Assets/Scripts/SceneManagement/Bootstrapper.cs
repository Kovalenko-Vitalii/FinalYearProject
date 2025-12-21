using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Bootstrapper : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return SceneManager.LoadSceneAsync("Core", LoadSceneMode.Additive);
        var orch = Object.FindFirstObjectByType<GameplayOrchestrator>();
        orch.EnterMenu();
    }
}

