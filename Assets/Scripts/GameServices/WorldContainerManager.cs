using UnityEngine;

public static class WorldContainerManager
{
    public static SaveWorldContainersData CaptureAll()
    {
        var data = new SaveWorldContainersData();

        var all = Object.FindObjectsByType<WorldContainer>(FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c == null) continue;

            var cs = new ContainerSave { containerId = c.ContainerId };

            foreach (var it in c.Inventory.items)
            {
                if (it == null || it.data == null) continue;
                if (it.amount <= 0) continue;

                cs.items.Add(new InventoryItemSave
                {
                    itemId = it.data.id,
                    amount = it.amount,
                    durability = it.currentDurability
                });
            }

            data.containers.Add(cs);
        }

        return data;
    }

    public static void RestoreAll(SaveWorldContainersData saved)
    {
        if (saved == null || saved.containers == null) return;

        var all = Object.FindObjectsByType<WorldContainer>(FindObjectsSortMode.None);

        var map = new System.Collections.Generic.Dictionary<string, WorldContainer>();
        foreach (var c in all)
            if (c != null && !string.IsNullOrWhiteSpace(c.ContainerId))
                map[c.ContainerId] = c;

        foreach (var cs in saved.containers)
        {
            if (cs == null || string.IsNullOrWhiteSpace(cs.containerId)) continue;
            if (!map.TryGetValue(cs.containerId, out var container)) continue;

            container.Inventory.items.Clear();

            if (cs.items == null) continue;

            foreach (var s in cs.items)
            {
                var itemData = ItemResolver.Resolve(s.itemId);
                if (itemData == null) continue;

                container.Inventory.AddItem(itemData, s.amount, s.durability);
            }
        }
    }
}
