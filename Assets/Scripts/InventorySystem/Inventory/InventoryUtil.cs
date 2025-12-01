using System.Linq;

public static class InventoryUtil
{
    public static int Count(Inventory inv, ItemData data)
        => inv?.items.Where(i => i.data == data).Sum(i => i.amount) ?? 0;

    public static InventoryItem MakeItem(Inventory inv, ItemData data)
        => inv?.items.FirstOrDefault(i => i.data == data);
}
