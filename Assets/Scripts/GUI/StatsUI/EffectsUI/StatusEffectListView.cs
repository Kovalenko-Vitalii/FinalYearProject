using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectListItemView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private TMP_Text durationText;

    private StatusEffect effect;
    private StatusEffectConfig config;
    private Action<StatusEffect> onClicked;

    public void Init(StatusEffect effect, StatusEffectConfig config, Action<StatusEffect> onClicked)
    {
        this.effect = effect;
        this.config = config;
        this.onClicked = onClicked;

        if (nameText != null)
            nameText.text = config != null ? config.displayName : effect.Id.ToString();

        if (iconImage != null)
            iconImage.sprite = config != null ? config.icon : null;

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClick);

        UpdateDurationText();
    }

    private void Update()
    {
        UpdateDurationText();
    }

    private void UpdateDurationText()
    {
        if (durationText == null || effect == null)
            return;

        float t = Mathf.Max(0f, effect.Duration);

        int seconds = Mathf.CeilToInt(t);
        durationText.text = seconds > 0 ? $"{seconds}s" : "0s";


        if (float.IsInfinity(effect.Duration)) durationText.text = "∞";
    }

    private void HandleClick()
    {
        onClicked?.Invoke(effect);
    }
}

