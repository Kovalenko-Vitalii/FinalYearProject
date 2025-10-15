using UnityEngine;

[CreateAssetMenu(menuName = "Items/ConsumaableData")]
public class ConsumableData : ItemData
{
	public float hpRestore = 0;
	public float hungerRestore = 0;
	public float hydrationRestore = 0;
	public float temperatureRestore = 0;

	public float durability;
}
