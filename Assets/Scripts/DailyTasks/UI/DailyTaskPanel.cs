using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class DailyTaskPanel : MonoBehaviour
{
    [SerializeField] private Image conditionImage;
    [SerializeField] private Image rewardImage;
    [SerializeField] private Image progressImage;
    [SerializeField] private TextMeshProUGUI conditionAmountText;
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject completedPanel;

    protected DailyTaskInstance task { get; private set; }

    protected void SetTask(DailyTaskInstance task)
    {
        if (task == null) {
            Debug.LogError("Task is not valid!");
        }
        if (task.Definition == null) {
            Debug.LogError("TaskDefinition is not valid!");
        }
        if (task.Reward == null) {
            Debug.LogError("TaskReward is not valid!");
        }

        this.task = task;
    }

    protected void UpdateTaskInfo()
    {
        if (task != null) {
            var reward = task.Reward;
            var rewardDefinition = reward?.Definition;

            if (reward != null && rewardDefinition != null) {
                SetRewardIcon(rewardDefinition.ItemIcon);
            }

            SetConditionIcon(task.GetConditionIcon());

            if (reward != null) {
                SetRewardAmountText(reward.Amount);
            }

            SetConditionAmountText(task.ConditionAmount);
            SetProgressAlpha(task.TaskProgressAlpha);
        }
    }

    // Amounts
    protected void SetRewardAmountText(int amount)
    {
        if (rewardAmountText == null) return;

        rewardAmountText.SetText(amount.ToString());
    }

    protected void SetConditionAmountText(int amount)
    {
        if (conditionAmountText == null) return;

        conditionAmountText.SetText(amount.ToString());
    }

    // Icons
    protected void SetRewardIcon(Sprite icon)
    {
        if (rewardImage == null) return;

        rewardImage.sprite = icon;
    }

    protected void SetConditionIcon(Sprite icon)
    {
        if (conditionImage == null) return;

        conditionImage.sprite = icon;
    }

    // Progress
    protected void SetProgressText(string value)
    {
        if (progressText == null) return;

        progressText.SetText(value);
    }

    protected void SetProgressAlpha(float alpha)
    {
        if (progressImage == null) return;

        progressImage.fillAmount = alpha;
    }

    protected void SetCompleted(bool value)
    {
        if (completedPanel == null) return;

        completedPanel.SetActive(value);
    }
}