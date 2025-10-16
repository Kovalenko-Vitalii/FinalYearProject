using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenSubMenuButton : MonoBehaviour {
    [Header("Menu Settings")]
    public CanvasSwitcher.SubMenuType subMenuType;

    [Tooltip("Если true – вызывает SwapTabSubMenu вместо OpenTabSubMenu")]
    public bool swap = false;

    private void Awake() {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        var uiManager = FindAnyObjectByType<CanvasSwitcher>();
        if (uiManager == null) 
            return;

        if (swap)
            uiManager.SwitchTabSubMenu(subMenuType);
        else
            uiManager.OpenTabSubMenu(subMenuType);
    }
}
