using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectListItemView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

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
    }

    private void HandleClick()
    {
        onClicked?.Invoke(effect);
    }
}
