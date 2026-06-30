using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskWidget : DailyTaskPanel
{
    [SerializeField] private TextLocalizer descriptionText;

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
        SetTask(task);
        TrySubscribe();
        UpdateTaskInfo();
        UpdateProgress();
        UpdateTaskDescription();
        UpdateCompleted();
    }

    private void UpdateProgress()
    {
        string currentProgress = task.Progress.ToString();
        string targetProgress = task.Definition.ConditionAmount.ToString();
        string text = currentProgress + "/" + targetProgress;
        SetProgressText(text);
    }

    private void UpdateTaskDescription()
    {
        LocalizationItem item = task.Definition.DescriptionLocalizationItem;
        descriptionText.SetLocalizationItem(item);
        descriptionText.SetPlaceHolderLocalization(task);
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

    private bool ShouldComplete()
    {
        return task != null ? task.IsCompleted : false;
    }
}