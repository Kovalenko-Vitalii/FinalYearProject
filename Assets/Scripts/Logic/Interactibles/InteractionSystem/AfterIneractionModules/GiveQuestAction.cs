using UnityEngine;

// This action module gives player new quest
[CreateAssetMenu(menuName = "InteractActions/Give Quest")]
public class GiveQuestAction : InteractAction
{
    [SerializeField] string questId;
    public override void Execute(InteractContext ctx)
    {
        var questManager = QuestManager.Instance;
        if (questManager && questId != "")
            questManager.StartQuest(questId);
    }
}
