using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskCompletedMenu : DailyTaskPanel
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private Image progressBar;

    [Header("Visibility")]
    [SerializeField] private float showTime = 5f;
    private float currentShowTime = 0f;

    private int lastProgressAdded;

    [SerializeField] private float progressLerpSpeed = 1f;
    private float animationAlpha = 0f;

    private bool isOpened = false;

    private void OnEnable()
    {
        DailyTaskInstance.onTaskProgressAdded += OnTaskProgressAdded;
        DailyTaskInstance.onTaskCompleted += OnTaskCompleted;
    }

    private void OnDisable()
    {
        DailyTaskInstance.onTaskProgressAdded -= OnTaskProgressAdded;
        DailyTaskInstance.onTaskCompleted -= OnTaskCompleted;
    }

    private void Update()
    {
        if (!isOpened) return;

        UpdateProgress();

        currentShowTime += Time.deltaTime;
        if (currentShowTime < showTime) return;

        Close();
    }

    public void Open()
    {
        if (isOpened) return;

        slidePanel.Display();
        isOpened = true;
    }

    public void Close()
    {
        if (!isOpened) return;

        SetTask(null);
        slidePanel.Hide();
        isOpened = false;
    }

    private void OnTaskProgressAdded(DailyTaskInstance task, int progress)
    {
        if (this.task != null && task != this.task) return;

        lastProgressAdded = progress;
    }

    private void OnTaskCompleted(DailyTaskInstance task)
    {
        Open();
        SetTask(task);
        UpdateTaskInfo();
        ResetShowTime();
        ResetProgressLerpApha();
        SetCompleted(false);
    }

    private void UpdateProgress()
    {
        int maxProgress = task.Progress;
        int minProgress = maxProgress - lastProgressAdded;

        animationAlpha += progressLerpSpeed * Time.deltaTime;
        animationAlpha = Mathf.Clamp01(animationAlpha);

        float currentProgress = Mathf.Lerp(minProgress, maxProgress, animationAlpha);
        float currentProgressAlpha = Mathf.Lerp((float)minProgress / maxProgress, 1, animationAlpha);

        progressBar.fillAmount = currentProgressAlpha;
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
}