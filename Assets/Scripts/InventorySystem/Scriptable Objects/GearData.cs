using UnityEngine;

[CreateAssetMenu(menuName = "Items/GearData")]
public class GearData : ItemData {
	public float temperatureResist = 0;
	public float damageResist = 0;
	

	public enum GearSlot {
		Head,
		Chest,
		Legs,
		Boots,
	}

	public GearSlot slot;
}
