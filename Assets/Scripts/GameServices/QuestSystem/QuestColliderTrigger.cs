using UnityEngine;

// This script is responsible for adding quest to player or triggering event
// of entering some specific area
public class QuestColliderTrigger : MonoBehaviour
{
    bool isUsed;
    [SerializeField] string questId;
    [SerializeField] string zoneId;

    // If there is questId it will automatically work as one time use "quest giver"
    private void OnTriggerEnter(Collider other)
    {
        if (isUsed && questId != null) return;
        if (!other.CompareTag("Player")) return;

        if (zoneId != null)
        {
            GameEvents.RaiseZoneEntered(zoneId); // Triggering event
        }
            
        if (questId != null)
        {
            QuestManager.Instance.StartQuest(questId); // Giving quest
            isUsed = true;
        }     
    }
}