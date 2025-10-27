using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour
{
    [Header("Links to UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buttonEquip;
    [SerializeField] private Button buttonDelete;

    [SerializeField] private TextMeshProUGUI hpStat;
    [SerializeField] private TextMeshProUGUI atkStat;
    [SerializeField] private TextMeshProUGUI defStat;
    [SerializeField] private TextMeshProUGUI magStat;
    [SerializeField] private TextMeshProUGUI evaStat;
    [SerializeField] private TextMeshProUGUI acuStat;
    [SerializeField] private TextMeshProUGUI crtStat;

    [Header("Colors for comparison")]
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;

    public void SetItem(InventoryItem item, Inventory source)
    {
        if (icon != null)
            icon.sprite = item.data.icon;

        if (itemName != null)
            itemName.text = item.data.itemName;

        if (itemDescription != null)
            itemDescription.text = item.data.description;


        if (item.data is GearData gearData)
        {

        }
        else if (item.data is ConsumableData consumableData)
        {

        }

        var uiManager = Object.FindAnyObjectByType<CanvasSwitcher>();
        if (buttonDelete != null)
        {
            buttonDelete.onClick.RemoveAllListeners();
            /*
            var confirmUI = Object.FindAnyObjectByType<ConfirmUI>();

            buttonDelete.onClick.AddListener(() => {
                uiManager.OpenTabSubMenu(UIManager.SubMenuType.Confirmation);
                confirmUI.Set(
                    onYes: () => {
                        source.RemoveItem(item.data, item.amount);
                        var ui = Object.FindAnyObjectByType<InventoryUI>();
                        if (ui != null) ui.Refresh();
                        uiManager.CloseLastMenu();
                        uiManager.CloseLastMenu();

                        var gearUI = FindAnyObjectByType<GearUI>();
                        if (gearUI != null) gearUI.Refresh();
                    },
                    onNo: () => { uiManager.CloseLastMenu(); }
                );
            });
            */
        }
        if (buttonEquip != null)
        {
            buttonEquip.onClick.RemoveAllListeners();
            buttonEquip.onClick.AddListener(() => {
                var inventoryManager = InventoryManager.Instance;

                if (item.data is GearData gearData)
                {
                    var oldGear = inventoryManager.playerEquipment.Equip(gearData);

                    source.RemoveItem(item.data, 1);

                    if (oldGear != null)
                        source.AddItem(oldGear, 1);

                    /*
                    var gearUI = FindAnyObjectByType<GearUI>();
                    if (gearUI != null)
                        gearUI.Refresh();
                    */

                    foreach (var invUI in FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
                    {
                        invUI.Refresh();
                    }

                    uiManager.CloseLastMenu();
                }
            });
        }
    }

    private void ShowStat(TextMeshProUGUI textField, float value, string label, ItemData newItem)
    {
        if (textField == null) return;

        string result = $"{label}: {value}";
        textField.color = neutralColor;

        // Если это шмотка, то сравниваем с надетым
        if (newItem is GearData gearData)
        {
            /*
            var equipped = InventoryManager.Instance.playerEquipment.GetEquipped(gearData.slot);
            if (equipped != null)
            {
                float oldValue = 0;
                switch (label)
                {
                    case "HP": oldValue = equipped.HP; break;
                    case "ATK": oldValue = equipped.ATK; break;
                    case "DEF": oldValue = equipped.DEF; break;
                    case "MAG": oldValue = equipped.MAG; break;
                    case "EVA": oldValue = equipped.EVA; break;
                    case "ACU": oldValue = equipped.ACU; break;
                    case "CRT": oldValue = equipped.CRT; break;
                }

                float diff = value - oldValue;
                if (diff > 0)
                {
                    result += $" (<color=#{ColorUtility.ToHtmlStringRGB(positiveColor)}>+{diff}</color>)";
                }
                else if (diff < 0)
                {
                    result += $" (<color=#{ColorUtility.ToHtmlStringRGB(negativeColor)}>{diff}</color>)";
                }
            }
            */
        }
        textField.text = result;
    }
}
