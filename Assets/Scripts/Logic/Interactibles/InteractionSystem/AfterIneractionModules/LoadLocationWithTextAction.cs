using UnityEngine;

// This class represents after interaction action that loads scene
[CreateAssetMenu(menuName = "InteractActions/Load Location With Text")]
public class LoadLocationWithTextAction : InteractAction
{
    [SerializeField] private string sceneName;
    [SerializeField] private string spawnId = "Start";

    [TextArea(5, 12)]
    [SerializeField] private string loadingText;

    [SerializeField] private AudioClip sound;

    public override void Execute(InteractContext ctx)
    {
        if (GameplayOrchestrator.Instance == null) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;

        GameplayOrchestrator.Instance.SetLoadingNarrative(loadingText, sound);
        GameplayOrchestrator.Instance.LoadLocation(sceneName, spawnId);
    }
}