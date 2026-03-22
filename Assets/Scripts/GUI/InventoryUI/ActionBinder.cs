using System.Linq;
using TMPro;
using UnityEngine.UI;

public static class ActionBinder
{
    public static void BindFixedButtons(
        Button dropButton,
        Button primaryButton,
        Button actionsButton,
        InventoryItem invItem,
        Inventory source,
        System.Action afterActionRefresh,
        string primaryFallbackLabel = "Use")
    {
        if (dropButton != null) { dropButton.onClick.RemoveAllListeners(); dropButton.gameObject.SetActive(false); }
        if (primaryButton != null) { primaryButton.onClick.RemoveAllListeners(); primaryButton.gameObject.SetActive(false); }
        if (actionsButton != null) { actionsButton.onClick.RemoveAllListeners(); actionsButton.gameObject.SetActive(false); }

        if (invItem == null || invItem.data is not IItemActionProvider provider)
            return;

        var im = InventoryManager.Instance;

        var ctx = new ItemActionContext
        {
            source = source,
            item = invItem,
            equippedItems = im != null ? im.playerEquippedItems : null
        };

        var actions = provider.GetActions(ctx).ToList();

        if (dropButton != null)
        {
            var drop = actions.Where(a => a.slot == ActionSlot.Drop).FirstOrDefault();
            if (drop.execute != null)
            {
                dropButton.gameObject.SetActive(true);
                dropButton.interactable = drop.interactable;
                dropButton.onClick.AddListener(() => { drop.execute?.Invoke(); afterActionRefresh?.Invoke();});
            }
        }

        if (primaryButton != null)
        {
            var hold = primaryButton.GetComponent<HoldToUse>();
            hold?.ClearBinding();

            var primary = actions.FirstOrDefault(a => a.slot == ActionSlot.Use);
            if (primary.execute != null)
            {
                primaryButton.gameObject.SetActive(true);
                primaryButton.interactable = primary.interactable;

                var label = primaryButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                    label.text = string.IsNullOrEmpty(primary.label) ? primaryFallbackLabel : primary.label;

                var itemData = invItem.data as ItemData;
                float duration = itemData != null ? itemData.useDuration : 0f;

                primaryButton.onClick.RemoveAllListeners();

                if (hold != null && duration > 0f)
                {
                    hold.Setup(
                        duration,
                        () =>
                        {
                            primary.execute?.Invoke();
                            afterActionRefresh?.Invoke();
                        },
                        primary.holdStartSound,
                        primary.holdStartSoundId
                    );
                }
                else
                {
                    primaryButton.onClick.AddListener(() =>
                    {
                        primary.execute?.Invoke();
                        afterActionRefresh?.Invoke();
                    });
                }
            }
        }

    }
}
