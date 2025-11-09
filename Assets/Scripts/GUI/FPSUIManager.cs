using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSUIManager : MonoBehaviour
{
    public TextMeshProUGUI underCrosshairLabel;
    public Image radialProgressBar;
    public void SetUnderCrosshairLabel(string text)
    {
        underCrosshairLabel.text = string.IsNullOrEmpty(text) ? "" : text;
    }
}
