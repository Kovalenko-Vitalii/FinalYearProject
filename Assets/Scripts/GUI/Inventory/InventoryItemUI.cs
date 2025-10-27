using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
	[SerializeField] private Image icon;
	[SerializeField] private TextMeshProUGUI itemName;
	[SerializeField] private TextMeshProUGUI amountText;
	[SerializeField] private TextMeshProUGUI priceText;
	[SerializeField] private Button button;

	[Header("Highlight")]
	[SerializeField] private GameObject selectionHighlight;

	private Inventory sourceInventory;
	private InventoryItem currentItem;

	public InventoryItem CurrentItem => currentItem;

	public void SetItem(InventoryItem item, Inventory source)
	{
		currentItem = item;
		this.sourceInventory = source;

		if (icon != null)
			icon.sprite = item.data.icon;

		if (itemName != null)
			itemName.text = item.data.itemName;

		if (amountText != null)
			amountText.text = item.amount.ToString();

		if (selectionHighlight != null)
			selectionHighlight.SetActive(false);

		if (button != null)
		{
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => OnItemClicked());
		}
	}

	private void OnItemClicked()
	{
		var manager = InventoryManager.Instance;

		manager.SelectItem(currentItem, sourceInventory);

		foreach (var ui in Object.FindObjectsByType<InventoryItemUI>(FindObjectsSortMode.None))
			ui.SetHighlight(false);

		SetHighlight(true);

		var uiManager = Object.FindAnyObjectByType<CanvasSwitcher>();
		var infoUI = Object.FindAnyObjectByType<ItemInfoUI>();
		if (uiManager != null)
		{
			if (infoUI != null)
				infoUI.SetItem(currentItem, sourceInventory);
		}
	}

	public void SetHighlight(bool active)
	{
		if (selectionHighlight != null)
			selectionHighlight.SetActive(active);
	}
}
