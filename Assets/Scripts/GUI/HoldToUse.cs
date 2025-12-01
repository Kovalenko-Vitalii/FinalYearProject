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

    public void Setup(float duration, Action onComplete)
    {
        this.duration = duration;
        this.onComplete = onComplete;
        ResetUI();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (duration <= 0f || onComplete == null)
            return;

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
