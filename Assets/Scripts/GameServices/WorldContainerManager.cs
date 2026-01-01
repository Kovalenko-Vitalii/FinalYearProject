using UnityEngine;

public static class WorldContainerManager
{
    // Capturing data from all containers
    // Maybe I should create capture and restore functions to world container
    public static SaveWorldContainersData CaptureAll()
    {
        var data = new SaveWorldContainersData();


        // Finding all containers and iterating each of them
        var all = Object.FindObjectsByType<WorldContainer>(FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c == null) continue;

            // Creating save for container
            var cs = new ContainerSave { containerId = c.ContainerId };

            // Adding each item to it
            foreach (var it in c.Inventory.items)
            {
                if (it == null || it.data == null || it.amount <= 0) continue;

                cs.items.Add(new InventoryItemSave
                {
                    itemId = it.data.id,
                    amount = it.amount,
                    durability = it.currentDurability
                });
            }
              
            // Adding to list of containers
            data.containers.Add(cs);
        }

        return data;
    }

    // Restoring content`s of all containers
    public static void RestoreAll(SaveWorldContainersData saved)
    {
        if (saved == null || saved.containers == null) return;

        // Finding all containers on map
        var all = Object.FindObjectsByType<WorldContainer>(FindObjectsSortMode.None);

        // Creating a dictionary of containers and their id`s corresponding
        var map = new System.Collections.Generic.Dictionary<string, WorldContainer>();
        foreach (var c in all)
            if (c != null && !string.IsNullOrWhiteSpace(c.ContainerId))
                map[c.ContainerId] = c;

        // Adding items to containers from save
        foreach (var cs in saved.containers)
        {
            if (cs == null || string.IsNullOrWhiteSpace(cs.containerId) ||
                !map.TryGetValue(cs.containerId, out var container) || cs.items == null) continue;

            // Cleaning all items
            container.Inventory.items.Clear();

            // Iterating all items in save and adding them
            foreach (var s in cs.items)
            {
                var itemData = ItemResolver.Resolve(s.itemId);
                if (itemData == null) continue;

                container.Inventory.AddItem(itemData, s.amount, s.durability);
            }
        }
    }
}
