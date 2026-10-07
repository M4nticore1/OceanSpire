using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class DailyTaskPanel : MonoBehaviour
{
    [SerializeField] private Image conditionImage;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI conditionAmount;
    [SerializeField] private TextMeshProUGUI rewardAmount;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject completedPanel;

    protected DailyTaskInstance task { get; private set; }

    protected void SetTask(DailyTaskInstance task)
    {
        if (task == null) {
            Debug.LogError("Task is not valid!");
        }
        if (task.Definition == null) {
            Debug.LogError("Task Definition is not valid!");
        }
        if (task.Reward == null) {
            Debug.LogError("Task Reward is not valid!");
        }

        this.task = task;
    }

    protected void UpdateTaskInfo()
    {
        if (task == null) return;
        if (task.Definition == null) return;
        if (task.Reward == null) return;
        if (task.Reward.Definition == null) return;

        conditionImage.sprite = task.Definition.ConditionImage;
        rewardImage.sprite = task.Reward.Definition.ItemIcon;

        conditionAmount.SetText(task.Definition.ConditionAmount.ToString());
        rewardAmount.SetText(task.Reward.Amount.ToString());
    }

    protected void SetProgressText(string value)
    {
        if (progressText == null) return;

        progressText.SetText(value);
    }

    protected void SetCompleted(bool value)
    {
        if (completedPanel == null) return;

        completedPanel.SetActive(value);
    }
}