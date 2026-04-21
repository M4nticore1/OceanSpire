using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskWidget : MonoBehaviour
{
    [SerializeField] private Image conditionImage;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI conditionAmount;
    [SerializeField] private TextMeshProUGUI rewardAmount;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextLocalizer descriptionText;
    [SerializeField] private GameObject completedPanel;

    private DailyTaskInstance task;
    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
        UpdateCompleted();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    public void Init(DailyTaskInstance task)
    {
        conditionImage.sprite = task.Definition.ConditionImage;
        rewardImage.sprite = task.Definition.Reward.ItemData.ItemIcon;

        conditionAmount.SetText(task.Definition.ConditionAmount.ToString());
        rewardAmount.SetText(task.Definition.Reward.Amount.ToString());

        this.task = task;

        TrySubscribe();
        UpdateProgress();
        UpdateTaskDescription();
        UpdateCompleted();
    }

    private void UpdateProgress()
    {
        string currentProgress = task.Progress.ToString();
        string targetProgress = task.Definition.ConditionAmount.ToString();
        string text = currentProgress + "/" + targetProgress;
        progressText.SetText(text);
    }

    private void UpdateTaskDescription()
    {
        LocalizationItem item = task.Definition.DescriptionLocalizationItem;
        descriptionText.SetLocalizationItem(item);
        descriptionText.SetPlaceHolderLocalization(task);
        descriptionText.UpdateText();
    }

    private void OnProgressChanged()
    {
        UpdateProgress();
        UpdateCompleted();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (task == null) return;

        task.onProgressChanged += OnProgressChanged;
        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (task == null) return;

        task.onProgressChanged -= OnProgressChanged;
        isSubscribed = false;
    }

    private void UpdateCompleted()
    {
        SetCompleted(ShouldComplete());
    }

    private void SetCompleted(bool value)
    {
        completedPanel.SetActive(value);
    }

    private bool ShouldComplete()
    {
        return task != null ? task.IsCompleted : false;
    }
}