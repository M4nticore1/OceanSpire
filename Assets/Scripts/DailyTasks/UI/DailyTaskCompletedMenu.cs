using UnityEngine;

public class DailyTaskCompletedMenu : DailyTaskPanel
{
    [SerializeField] private SlideAnimatedPanel slidePanel;

    [Header("Visibility")]
    [SerializeField] private float showTime = 5f;
    private float currentShowTime = 0f;

    private int lastProgressAdded;

    [SerializeField] private float progressLerpSpeed = 1f;
    private float animationAlpha = 0f;

    private bool isOpened => slidePanel.IsShown;

    private void OnEnable()
    {
        DailyTaskInstance.OnTaskProgressAdded += HandleTaskProgressAdded;
        DailyTaskInstance.OnTaskCompleted += HandleTaskCompleted;
    }

    private void OnDisable()
    {
        DailyTaskInstance.OnTaskProgressAdded -= HandleTaskProgressAdded;
        DailyTaskInstance.OnTaskCompleted -= HandleTaskCompleted;
    }

    private void Update()
    {
        if (isOpened) {
            UpdateProgress();

            currentShowTime += Time.deltaTime;
            if (currentShowTime >= showTime) {
                Hide();
            }
        }
    }

    public void Show()
    {
        if (isOpened) return;

        slidePanel.Show();
    }

    public void Hide()
    {
        if (!isOpened) return;

        slidePanel.Hide();
    }

    private void UpdateProgress()
    {
        var maxProgress = task.TaskProgress;
        var minProgress = maxProgress - lastProgressAdded;

        animationAlpha += progressLerpSpeed * Time.deltaTime;
        animationAlpha = Mathf.Clamp01(animationAlpha);

        var currentProgress = Mathf.Lerp(minProgress, maxProgress, animationAlpha);
        var currentProgressAlpha = Mathf.Lerp((float)minProgress / maxProgress, 1, animationAlpha);

        SetProgressAlpha(currentProgressAlpha);
        SetProgressText(((int)currentProgress).ToString() + "/" + maxProgress.ToString());

        if (animationAlpha >= 1f) {
            SetCompleted(true);
        }
    }

    private void ResetShowTime()
    {
        currentShowTime = 0;
    }

    private void ResetProgressLerpApha()
    {
        animationAlpha = 0f;
    }

    // Events
    private void HandleTaskProgressAdded(DailyTaskInstance task, int progress)
    {
        if (this.task != null && task != this.task) return;

        lastProgressAdded = progress;
    }

    private void HandleTaskCompleted(DailyTaskInstance task)
    {
        Show();
        SetTask(task);
        UpdateTaskInfo();
        ResetShowTime();
        ResetProgressLerpApha();
        SetCompleted(false);
    }
}