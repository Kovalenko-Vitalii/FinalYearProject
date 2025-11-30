using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusEffectUIRoot : MonoBehaviour
{
    [Header("Body Part Buttons")]
    [SerializeField] private BodyPartButtonView[] bodyPartButtons;

    [Header("Effects List")]
    [SerializeField] private Transform effectsListContent;
    [SerializeField] private StatusEffectListItemView effectItemPrefab;

    [Header("Details Panel")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private string nothingSelectedTitle = "Nothing selected";
    [SerializeField] private string nothingSelectedDescription = "";

    [Header("Part Title")]
    [SerializeField] private TMP_Text partTitleText;
    [SerializeField] private string noPartSelectedText = "No part selected";

    private string GetPartDisplayName(BodyPart part)
    {
        return part switch
        {
            BodyPart.Head => "Head",
            BodyPart.Torso => "Torso",
            BodyPart.LeftArm => "Left arm",
            BodyPart.RightArm => "Right arm",
            BodyPart.LeftLeg => "Left leg",
            BodyPart.RightLeg => "Right leg",
            _ => part.ToString()
        };
    }


    private BodyPart? currentPart;
    private StatusEffect currentSelectedEffect;

    private StatusEffectManager EffectManager => StatusEffectManager.Instance;

    private void OnEnable()
    {
        foreach (var btn in bodyPartButtons)
            btn.Clicked += OnBodyPartClicked;

        if (EffectManager != null)
        {
            EffectManager.OnEffectAdded += OnEffectChanged;
            EffectManager.OnEffectRemoved += OnEffectChanged;
        }

        RefreshBodyPartButtons();
        ClearSelection(); 
        BuildEmptyEffectList();
    }

    private void OnDisable()
    {
        foreach (var btn in bodyPartButtons)
        {
            btn.Clicked -= OnBodyPartClicked;
        }

        if (EffectManager != null)
        {
            EffectManager.OnEffectAdded -= OnEffectChanged;
            EffectManager.OnEffectRemoved -= OnEffectChanged;
        }
    }

    private void OnEffectChanged(StatusEffect _)
    {
        RefreshBodyPartButtons();

        if (currentPart.HasValue)
            BuildEffectList(currentPart.Value);

        if (currentSelectedEffect != null && currentSelectedEffect.IsFinished)
            ClearSelection();
    }

    private void RefreshBodyPartButtons()
    {
        if (EffectManager == null) return;

        foreach (var btn in bodyPartButtons)
        {
            var list = EffectManager.GetEffectsForPart(btn.Part);
            bool has = list != null && list.Count > 0;
            btn.SetHasEffects(has);
        }

        if (currentPart.HasValue)
        {
            var list = EffectManager.GetEffectsForPart(currentPart.Value);
            if (list == null || list.Count == 0)
            {
                currentPart = null;
                ClearSelection();
                BuildEmptyEffectList();
            }
        }
    }


    private void OnBodyPartClicked(BodyPart part)
    {
        currentPart = part;
        currentSelectedEffect = null;

        if (partTitleText != null)
            partTitleText.text = GetPartDisplayName(part);

        BuildEffectList(part);
        ClearDetailsOnly();
    }

    private void BuildEffectList(BodyPart part)
    {
        if (EffectManager == null || effectsListContent == null || effectItemPrefab == null)
            return;

        for (int i = effectsListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(effectsListContent.GetChild(i).gameObject);
        }

        IReadOnlyList<StatusEffect> list = EffectManager.GetEffectsForPart(part);
        foreach (var effect in list)
        {
            var cfg = StatusEffectRules.GetConfig(effect.Id);
            var item = Instantiate(effectItemPrefab, effectsListContent);
            item.Init(effect, cfg, OnEffectItemClicked);
        }
    }

    private void OnEffectItemClicked(StatusEffect effect)
    {
        currentSelectedEffect = effect;
        var cfg = StatusEffectRules.GetConfig(effect.Id);

        if (cfg != null)
        {
            if (titleText != null)
                titleText.text = cfg.displayName;
            if (descriptionText != null)
                descriptionText.text = cfg.description;
        }
        else
        {
            if (titleText != null)
                titleText.text = effect.Id.ToString();
            if (descriptionText != null)
                descriptionText.text = "";
        }
    }

    private void ClearSelection()
    {
        currentSelectedEffect = null;
        currentPart = null;

        if (partTitleText != null)
            partTitleText.text = noPartSelectedText;

        if (titleText != null)
            titleText.text = nothingSelectedTitle;

        if (descriptionText != null)
            descriptionText.text = nothingSelectedDescription;
    }

    private void ClearDetailsOnly()
    {
        currentSelectedEffect = null;

        if (titleText != null)
            titleText.text = nothingSelectedTitle;

        if (descriptionText != null)
            descriptionText.text = nothingSelectedDescription;
    }

    private void BuildEmptyEffectList()
    {
        if (effectsListContent == null) return;

        for (int i = effectsListContent.childCount - 1; i >= 0; i--)
            Destroy(effectsListContent.GetChild(i).gameObject);
    }

}
