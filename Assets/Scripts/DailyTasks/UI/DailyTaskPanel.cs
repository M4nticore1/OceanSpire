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
        this.task = task;
    }

    protected void UpdateTaskInfo()
    {
        conditionImage.sprite = task.Definition.ConditionImage;
        rewardImage.sprite = task.Definition.Reward.ItemData.ItemIcon;

        conditionAmount.SetText(task.Definition.ConditionAmount.ToString());
        rewardAmount.SetText(task.Definition.Reward.Amount.ToString());
    }

    protected void SetProgressText(string value)
    {
        progressText.SetText(value);
    }

    protected void SetCompleted(bool value)
    {
        completedPanel.SetActive(value);
    }
}