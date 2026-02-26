using UnityEngine;

// This action module transits a player to another location
[CreateAssetMenu(menuName = "InteractActions/Load Location")]
public class LoadLocationAction : InteractAction
{
    [SerializeField] private string sceneName;
    [SerializeField] private string spawnId = "Start";

    public override void Execute(InteractContext ctx)
    {
        if (GameplayOrchestrator.Instance == null) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;

        GameplayOrchestrator.Instance.LoadLocation(sceneName, spawnId);
    }
}