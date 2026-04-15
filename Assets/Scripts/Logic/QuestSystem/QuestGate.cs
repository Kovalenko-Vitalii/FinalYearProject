using UnityEngine;

// Used to block inappropriate player interations
public class QuestGate : MonoBehaviour
{
    [SerializeField] private QuestData requiredActiveQuest;
    [SerializeField] private string lockedPrompt = "Try later";

    public string LockedPrompt => lockedPrompt;
    public QuestData RequiredActiveQuest => requiredActiveQuest;

    public bool IsPassed()
    {
        if (requiredActiveQuest == null)
            return true;

        if (QuestManager.Instance == null)
            return false;

        return QuestManager.Instance.IsQuestActive(requiredActiveQuest);
    }
}