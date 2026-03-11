using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    string TAG = "SceneLoader";
    public static SceneLoader Instance { get; private set; }

    string _currentContentScene; // Name of current loaded in content scene
 
    AsyncOperation _activeLoadOp; // Async operation for paralel loading

    public string CurrentContentScene => _currentContentScene;

    public float Progress { get; private set; } = 0f; // Float that indicated loading progress, used mainly for UI

    public bool IsLoading => _activeLoadOp != null && !_activeLoadOp.isDone; // Shows if scene loading at the moment

    private void Awake()
    {
        GameLog.Log(TAG, $"Awake() initiated");

        if (Instance != null && Instance != this) 
        {
            GameLog.Warning(TAG, $"Duplicate SceneLoader -> destroying id={GetInstanceID()}");
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        GameLog.Log(TAG, $"Awake() finished. Singleton set");
    }

    // Load content scene and activate automatically when loaded
    public Coroutine LoadContent(string sceneName)
    {
        if (_currentContentScene == sceneName)
        {
            var s = SceneManager.GetSceneByName(sceneName);
            if (s.IsValid() && s.isLoaded)
            {
                SceneManager.SetActiveScene(s);
                Progress = 1f;
                return null;
            }
        }

        if (IsLoading)
        {
            GameLog.Warning(TAG, $"LoadContent ignored: already loading");
            return null;
        }
        GameLog.Log(TAG, $"LoadContent('{sceneName}') initiated ");

        Progress = 0f; // Resetting load progress
      
        _activeLoadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive); // Adding loading process to async and activate it        
        _activeLoadOp.allowSceneActivation = true;

        return StartCoroutine(FinishSwapRoutine(sceneName));
    }

    // Save as LoadContent but user can choose when to finally load scene
    public AsyncOperation LoadContentAsync(string sceneName, bool allowSceneActivation)
    {
        if (_currentContentScene == sceneName)
        {
            var s = SceneManager.GetSceneByName(sceneName);
            if (s.IsValid() && s.isLoaded)
            {
                SceneManager.SetActiveScene(s);
                Progress = 1f;
                return null;
            }
        }

        if (IsLoading)
        {
            GameLog.Warning(TAG, $"LoadContentAsync ignored: already loading");
            return _activeLoadOp;
        }

        GameLog.Log(TAG, $"AsyncLoadContent('{sceneName}') initiated ");

        Progress = 0f;

        _activeLoadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _activeLoadOp.allowSceneActivation = allowSceneActivation;

        StartCoroutine(FinishSwapRoutine(sceneName));
        return _activeLoadOp;
    }

    // Trigger scene activation
    public void ActivateLoadedScene()
    {
        if (_activeLoadOp != null)
        {
            GameLog.Log(TAG, "ActivateLoadedScene()");
            _activeLoadOp.allowSceneActivation = true;
        }
        else
            GameLog.Warning(TAG, "ActivateLoadedScene called but _activeLoadOp is null");
    }

    // This method used to wait for scene load and than switch it with current one
    private IEnumerator FinishSwapRoutine(string sceneName)
    {
        var op = _activeLoadOp;

        while (op != null && !op.isDone)
        {
            float raw = op.progress;
            Progress = Mathf.Clamp01(raw < 0.9f ? raw / 0.9f : 1f);
            yield return null;
        }

        GameLog.Log(TAG, $"FinishSwapRoutine BEGIN target='{sceneName}' prev='{_currentContentScene}'");

        // Waiting for loading and updating progress
        /*
        while (_activeLoadOp != null && !_activeLoadOp.isDone)
        {
            float raw = _activeLoadOp.progress;
            Progress = Mathf.Clamp01(raw < 0.9f ? raw / 0.9f : 1f);
            yield return null;
        }
        */
        var loadedScene = SceneManager.GetSceneByName(sceneName); // Making this scene active

        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
            GameLog.Log(TAG, $"SetActiveScene '{sceneName}'"); 
        }
        else
            GameLog.Error(TAG, $"Loaded scene '{sceneName}' is NOT valid");

        // Unloading previous scene
        if (!string.IsNullOrEmpty(_currentContentScene) && _currentContentScene != sceneName)
        {
            GameLog.Log(TAG, $"Unloading previous content '{_currentContentScene}'");
            yield return SceneManager.UnloadSceneAsync(_currentContentScene);
        }


        _currentContentScene = sceneName; // Setting loaded scene as current, updating progress

        Progress = 1f;
        _activeLoadOp = null;

        GameLog.Log(TAG, $"FinishSwapRoutine END currentContent='{_currentContentScene}' activeScene='{SceneManager.GetActiveScene().name}'");
    }
}
