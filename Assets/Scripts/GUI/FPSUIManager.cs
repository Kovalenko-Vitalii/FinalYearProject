using TMPro;
using UnityEngine;

public class FPSUIManager : MonoBehaviour
{
    public TextMeshProUGUI underCrosshairLabel;
    
    public void SetUnderCrosshairLabel(string text)
    {
        underCrosshairLabel.text = string.IsNullOrEmpty(text) ? "" : text;
    }
}
