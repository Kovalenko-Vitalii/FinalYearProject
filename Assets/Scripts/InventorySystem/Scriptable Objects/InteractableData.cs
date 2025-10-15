using UnityEngine;

[CreateAssetMenu(menuName = "Items/InteractableData")]
public class InteractableData : ItemData
{
	public float hpRestore = 0;
	public float hungerRestore = 0;
	public float hydrationRestore = 0;
	public float temperatureRestore = 0;

	public float durability;
}
