using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotRowUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TextMeshProUGUI titleText;

    private string _slotId;
    private SaveMenuUI _menu;

    public void Bind(SaveSlotMeta meta, SaveMenuUI menu)
    {
        _slotId = meta.id;
        _menu = menu;

        if (titleText != null)
            titleText.text = meta.displayName;

        if (button == null) button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(Delete);
        }
    }

    private void OnClick()
    {
        if (_menu != null && !string.IsNullOrEmpty(_slotId))
            _menu.LoadSlot(_slotId);
    }

    private void Delete()
    {
        if (_menu != null && !string.IsNullOrEmpty(_slotId))
            _menu.DeleteSlot(_slotId);
    }
}
