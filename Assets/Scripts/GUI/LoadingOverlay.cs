using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject pressAnyKeyRoot;

    [SerializeField] private float fadeTime = 0.2f;

    Coroutine _fade;

    private void Awake()
    {
        if (!group) group = GetComponentInChildren<CanvasGroup>(true);

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        if (progressSlider) progressSlider.value = 0f;

        if (pressAnyKeyRoot) pressAnyKeyRoot.SetActive(false);

        gameObject.SetActive(true);
    }

    public void Show()
    {
        transform.SetAsLastSibling();
        if (progressSlider) progressSlider.value = 0f;
        ShowPressAnyKey(false);
        FadeTo(1f, true);
    }

    public void Hide()
    {
        ShowPressAnyKey(false);
        FadeTo(0f, false);
    }

    public void SetProgress(float t)
    {
        if (!progressSlider) return;
        progressSlider.value = Mathf.Clamp01(t);
    }

    public void ShowPressAnyKey(bool show)
    {
        if (pressAnyKeyRoot) pressAnyKeyRoot.SetActive(show);
    }

    void FadeTo(float target, bool blockInput)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(FadeRoutine(target, blockInput));
    }

    IEnumerator FadeRoutine(float target, bool blockInput)
    {
        group.blocksRaycasts = blockInput;
        group.interactable = blockInput;

        float start = group.alpha;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.001f, fadeTime);
            group.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }

        group.alpha = target;

        if (Mathf.Approximately(target, 0f))
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    public IEnumerator ShowAndWait()
    {
        transform.SetAsLastSibling();

        if (progressSlider) progressSlider.value = 0f;
        ShowPressAnyKey(false);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (_fade != null)
            StopCoroutine(_fade);

        group.blocksRaycasts = true;
        group.interactable = true;

        float start = group.alpha;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.001f, fadeTime);
            group.alpha = Mathf.Lerp(start, 1f, t);
            yield return null;
        }

        group.alpha = 1f;
    }
}
