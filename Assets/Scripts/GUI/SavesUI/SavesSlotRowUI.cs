using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotRowUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI detailsText;

    private string _slotId;
    private SaveMenuUI _menu;

    public void Bind(SaveSlotMeta meta, SaveMenuUI menu)
    {
        _slotId = meta.id;
        _menu = menu;

        if (titleText != null)
            titleText.text = meta.displayName;

        if (detailsText != null)
        {
            var updated = new DateTime(meta.updatedUtcTicks, DateTimeKind.Utc).ToLocalTime();
            detailsText.text = $"{updated:yyyy-MM-dd HH:mm} | {meta.sceneName} | {meta.spawnId}";
        }

        if (button == null) button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (_menu != null && !string.IsNullOrEmpty(_slotId))
            _menu.LoadSlot(_slotId);
    }
}
