using System.Text;
using TMPro;
using UnityEngine;

public class QuestDetailsUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;

    public void Show(QuestData quest)
    {
        if (quest == null) return;

        if (root != null) root.SetActive(true);
        if (title != null) title.text = quest.title;

        if (description != null)
        {
            var sb = new StringBuilder();

            sb.AppendLine(quest.description);

            if (quest.steps != null && quest.steps.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Tasks: ");

                for (int i = 0; i < quest.steps.Count; i++)
                {
                    var step = quest.steps[i];
                    int progress = QuestManager.Instance.GetStepProgress(
                        quest.questId, i
                    );

                    sb.AppendLine($"• {step.description} ({progress}/{step.amount})");
                }
            }

            description.text = sb.ToString();
        }
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}