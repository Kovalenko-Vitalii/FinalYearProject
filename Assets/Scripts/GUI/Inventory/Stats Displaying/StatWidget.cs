using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatWidget : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI valueText;

    public void Bind(Sprite sprite, string value, float? diff,
                     Color positive, Color negative)
    {
        icon.sprite = sprite;
        if (diff.HasValue && Mathf.Abs(diff.Value) > 0.0001f)
        {
            var sign = diff.Value > 0 ? "+" : "";
            var hex = ColorUtility.ToHtmlStringRGB(diff.Value > 0 ? positive : negative);
            valueText.text = $"{value} <color=#{hex}>({sign}{diff.Value:0.##})</color>";
        }
        else
        {
            valueText.text = value;
        }
        gameObject.SetActive(true);
    }
}
