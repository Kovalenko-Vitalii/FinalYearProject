using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldToUse : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image radialImage;

    private float duration;
    private Action onComplete;
    private AudioClip sound;
    private UISoundId soundId;

    private bool isConfigured;
    private bool isHolding;
    private float timer;

    private Button cachedButton;

    private void Awake()
    {
        cachedButton = GetComponent<Button>();
        ResetProgress();
    }

    private void OnDisable()
    {
        ResetProgress();
    }

    public void Setup(float duration, Action onComplete, AudioClip sound, UISoundId soundId)
    {
        ClearBinding();

        if (duration <= 0f || onComplete == null)
            return;

        this.duration = duration;
        this.onComplete = onComplete;
        this.sound = sound;
        this.soundId = soundId;
        isConfigured = true;
    }

    public void ClearBinding()
    {
        isConfigured = false;
        duration = 0f;
        onComplete = null;
        sound = null;
        soundId = default;
        ResetProgress();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanStartHold())
            return;

        ResetProgress();

        SoundManager.Instance?.PlayUI(soundId, sound);

        isHolding = true;

        if (panel != null)
            panel.SetActive(true);

        if (radialImage != null)
            radialImage.fillAmount = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelHold();
    }

    private void Update()
    {
        if (!isHolding)
            return;

        if (!CanContinueHold())
        {
            CancelHold();
            return;
        }

        timer += Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(timer / duration);

        if (radialImage != null)
            radialImage.fillAmount = t;

        if (t >= 1f)
        {
            Action callback = onComplete;

            ResetProgress();
            callback?.Invoke();
        }
    }

    private bool CanStartHold()
    {
        if (!isConfigured)
            return false;

        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            return false;

        if (cachedButton != null && !cachedButton.interactable)
            return false;

        return true;
    }

    private bool CanContinueHold()
    {
        if (!isConfigured)
            return false;

        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            return false;

        if (cachedButton != null && !cachedButton.interactable)
            return false;

        return true;
    }

    private void CancelHold()
    {
        ResetProgress();
    }

    private void ResetProgress()
    {
        isHolding = false;
        timer = 0f;

        if (panel != null)
            panel.SetActive(false);

        if (radialImage != null)
            radialImage.fillAmount = 0f;
    }
}