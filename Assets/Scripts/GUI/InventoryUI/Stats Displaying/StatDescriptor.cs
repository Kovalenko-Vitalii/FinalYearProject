using UnityEngine;
public enum StatId
{
    // for cloth
    TemperatureResist, DamageResist, Weight,
    // for consumables
    HpRestore, HungerRestore, HydrationRestore, TemperatureRestore, Durability
}

[CreateAssetMenu(menuName = "GUI/Stats/Stat Descriptor")]
public class StatDescriptor : ScriptableObject
{
    public StatId id;
    public Sprite icon;
    public string unit;      
    public int priority = 0; 
    public bool hideIfZero = true;

    public string Format(float v) =>
        v % 1f == 0 ? ((int)v).ToString() : v.ToString("0.##");
}