using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyTasksMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private DailyTasksManager dailyTasksManager;
    [SerializeField] private DailyTaskWidget dailyTaskWidgetPrefab;
    [SerializeField] private LayoutGroup tasksLayoutGroup;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private TextLocalizer updateTasksText;

    private List<DailyTaskWidget> widgets = new();

    private bool isSubscribed = false;
    private bool areWidgetsSpawned = false;

    public event Action OnShown;
    public event Action OnHidden;

    private void OnEnable()
    {
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);

        dailyTasksManager.SetTasksViewed(true);
        TrySubscribe();
    }

    private void OnDisable()
    {
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);

        TryUnsubscribe();
    }

    private void Update()
    {
        updateTasksText.UpdateText();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        TryRemoveWidgets();
        TryCreateWidgets();
        InputStateManager.Instance.AddBlockTarget();

        OnShown?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        InputStateManager.Instance.RemoveBlockTarget();

        OnHidden?.Invoke();
    }

    private void TryCreateWidgets()
    {
        if (areWidgetsSpawned) return;

        CreateTaskWidgets();
    }

    private void CreateTaskWidgets()
    {
        for (int i = 0; i < DailyTasksManager.Instance.CurrentTasks.Count; i++) {
            CreateTaskWidget(DailyTasksManager.Instance.CurrentTasks[i]);
        }

        areWidgetsSpawned = true;
    }

    private void CreateTaskWidget(DailyTaskInstance task)
    {
        DailyTaskWidget widget = DailyTaskWidgetFactory.CreateWidget(dailyTaskWidgetPrefab, tasksLayoutGroup.transform, task);
        widgets.Add(widget);
    }

    private void TryRemoveWidgets()
    {
        if (areWidgetsSpawned) return;

        RemoveTaskWidgets();
    }

    private void RemoveTaskWidgets()
    {
        for (int i = widgets.Count - 1; i >= 0; i--) {
            Destroy(widgets[i].gameObject);
            widgets.RemoveAt(i);
        }

        areWidgetsSpawned = false;
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        dailyTasksManager.OnTasksCreated += OnTasksInited;
        dailyTasksManager.OnTasksViewedChanged += OnTasksViewedChanged;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        dailyTasksManager.OnTasksViewedChanged -= OnTasksViewedChanged;

        isSubscribed = false;
    }

    private void OnTasksInited()
    {
        RemoveTaskWidgets();
        CreateTaskWidgets();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }

    private void OnTasksViewedChanged(bool value)
    {
        dailyTasksManager.SetTasksViewed(false);
    }
}