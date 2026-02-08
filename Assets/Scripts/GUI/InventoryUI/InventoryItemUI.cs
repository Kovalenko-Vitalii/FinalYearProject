using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
	[SerializeField] private Image icon;
	[SerializeField] private TextMeshProUGUI itemName;
	[SerializeField] private TextMeshProUGUI amountText;
	[SerializeField] private TextMeshProUGUI weightText;
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

		if (weightText != null)
			weightText.text = (item.data.weight * item.amount).ToString() + " kg";

		if (amountText != null)
			if (item.amount > 1)
			{
				amountText.text = item.amount.ToString();
			}
			else amountText.text = "";



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

		// Sound integration
		SoundManager.Instance?.PlayUI(UISoundId.ItemClick, currentItem.data.onClickSound);
	}

	public void SetHighlight(bool active)
	{
		if (selectionHighlight != null)
			selectionHighlight.SetActive(active);
	}
}
