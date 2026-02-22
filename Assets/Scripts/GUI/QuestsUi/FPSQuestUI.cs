using System.Text;
using TMPro;
using UnityEngine;

public class FPSQuestUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    private void OnEnable()
    {
        var qm = QuestManager.Instance;
        if (qm == null)
        {
            Clear();
            return;
        }

        qm.ActiveQuestChanged += OnQuestChanged;
        qm.QuestProgressChanged += OnQuestProgress;

        OnQuestChanged(qm.ActiveQuest);
    }

    private void OnDisable()
    {
        var qm = QuestManager.Instance;
        if (qm == null) return;

        qm.ActiveQuestChanged -= OnQuestChanged;
        qm.QuestProgressChanged -= OnQuestProgress;
    }

    private void OnQuestChanged(QuestData quest)
    {
        Render(quest);
    }

    private void OnQuestProgress(QuestData quest)
    {
        Render(QuestManager.Instance != null ? QuestManager.Instance.ActiveQuest : null);
    }

    private void Render(QuestData quest)
    {
        if (quest == null)
        {
            Clear();
            return;
        }

        if (titleText != null)
            titleText.text = quest.title;

        if (bodyText != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(quest.description);

            if (quest.steps != null && quest.steps.Count > 0)
            {
                sb.AppendLine();
                for (int i = 0; i < quest.steps.Count; i++)
                {
                    var step = quest.steps[i];
                    int progress = QuestManager.Instance.GetStepProgress(quest.questId, i);
                    sb.AppendLine($"• {step.description} ({progress}/{step.amount})");
                }
            }

            bodyText.text = sb.ToString();
        }
    }

    private void Clear()
    {
        if (titleText != null) titleText.text = "";
        if (bodyText != null) bodyText.text = "";
    }
}