using System;
using static ItemData;

// This class contains events used for quest steps
public static class GameEvents
{
    // For future
    public static event Action Slept;

    public static event Action<string, int> ItemPicked;
    public static event Action<string> ZoneEntered;
    public static event Action<string> Interacted;
    public static event Action<string, ItemTag, int> Consumed;

    public static void RaiseSlept() => Slept?.Invoke();
    public static void RaiseConsumed(string itemId, ItemTag tag, int amount = 1)
        => Consumed?.Invoke(itemId, tag, amount);
    public static void RaiseItemPicked(string itemId, int amount) => ItemPicked?.Invoke(itemId, amount);
    public static void RaiseZoneEntered(string zoneId) => ZoneEntered?.Invoke(zoneId);
    public static void RaiseInteracted(string id) => Interacted?.Invoke(id);
}
