using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private string _currentContentScene;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Coroutine LoadContent(string sceneName)
    {
        return StartCoroutine(LoadContentRoutine(sceneName));
    }

    private IEnumerator LoadContentRoutine(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        if (!string.IsNullOrEmpty(_currentContentScene) && _currentContentScene != sceneName)
        {
            yield return SceneManager.UnloadSceneAsync(_currentContentScene);
        }

        _currentContentScene = sceneName;
    }
}
