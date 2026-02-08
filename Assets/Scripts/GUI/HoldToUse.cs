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

    private bool isHolding;
    private float timer;
    AudioClip sound;
    UISoundId soundId;

    public void Setup(float duration, Action onComplete, AudioClip sound, UISoundId soundId)
    {
        this.duration = duration;
        this.onComplete = onComplete;
        this.sound = sound;
        this.soundId = soundId;

        Debug.Log($"[HoldToUse.Setup] duration={duration} sound={(sound ? sound.name : "NULL")} id={soundId}");

        ResetUI();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (duration <= 0f || onComplete == null)
            return;

        SoundManager.Instance?.PlayUI(soundId, sound);

        isHolding = true;
        timer = 0f;
        if (panel) panel.SetActive(true);
        if (radialImage) radialImage.fillAmount = 0f;
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
        if (!isHolding) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        if (radialImage) radialImage.fillAmount = t;

        if (t >= 1f)
        {
            isHolding = false;
            if (panel) panel.SetActive(false);
            onComplete?.Invoke();
            ResetUI();
        }
    }

    private void CancelHold()
    {
        if (!isHolding) return;
        isHolding = false;
        ResetUI();
    }

    private void ResetUI()
    {
        if (panel) panel.SetActive(false);
        if (radialImage) radialImage.fillAmount = 0f;
        timer = 0f;
    }
}
