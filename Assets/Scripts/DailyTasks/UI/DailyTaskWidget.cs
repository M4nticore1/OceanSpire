using UnityEngine;

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
        if (task == null) {
            Debug.LogError("Task is not valid!");
        }

        SetTask(task);
        TrySubscribe();
        UpdateTaskInfo();
        UpdateProgress();
        UpdateTaskDescription();
        UpdateCompleted();
    }

    private void UpdateProgress()
    {
        var currentProgress = task.TaskProgress.ToString();
        var targetProgress = task.ConditionAmount.ToString();
        var text = currentProgress + "/" + targetProgress;
        SetProgressText(text);
    }

    private void UpdateTaskDescription()
    {
        if (descriptionText == null) return;

        if (task != null) {
            var definition = task?.Definition;
            var localization = definition?.DescriptionLocalizationItem;

            descriptionText.SetLocalizationItem(localization);
            descriptionText.SetPlaceHolderLocalization(task);
        }
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

        task.OnProgressChanged += OnProgressChanged;
        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (task == null) return;

        task.OnProgressChanged -= OnProgressChanged;
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