using TMPro;
using UnityEngine;

// This script used for subtitle visualisation 
public class SubtitleView : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text subtitleText;

    public void Show(LineRequest req)
    {
        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        if (subtitleText != null)
            subtitleText.text = req?.line != null ? req.line.text : "";
    }

    public void Hide()
    {
        if (group != null) group.alpha = 0f;
        if (subtitleText != null) subtitleText.text = "";
    }
}
