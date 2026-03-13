using UnityEngine;

public class AILootDrop : MonoBehaviour
{
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.25f, 0f);
    [SerializeField] private float randomRadius = 0.3f;
    [SerializeField] private float impulseStrength = 1.5f;

    private bool alreadyDropped;

    public void Drop(AIConfig config)
    {
        if (alreadyDropped)
            return;

        alreadyDropped = true;

        if (config == null)
            return;

        if (config.dropItem == null)
            return;

        if (Random.value > config.dropChance)
            return;

        int amount = Random.Range(config.dropMinAmount, config.dropMaxAmount + 1);
        if (amount <= 0)
            return;

        if (WorldObjectSpawner.Instance == null)
        {
            Debug.LogError("WorldObjectSpawner.Instance is missing", this);
            return;
        }

        Vector3 spawnPos = transform.position + dropOffset;
        spawnPos += new Vector3(
            Random.Range(-randomRadius, randomRadius),
            0f,
            Random.Range(-randomRadius, randomRadius)
        );

        Vector3 impulse = new Vector3(
            Random.Range(-0.5f, 0.5f),
            1f,
            Random.Range(-0.5f, 0.5f)
        ).normalized * impulseStrength;

        WorldObjectSpawner.Instance.SpawnItem(
            config.dropItem,
            amount,
            0f,
            spawnPos,
            Quaternion.identity,
            impulse
        );
    }
}