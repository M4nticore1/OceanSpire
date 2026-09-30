using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyTasksMenu : MonoBehaviour, IOpenable
{
    [Header("Main")]
    [SerializeField] private DailyTasksManager dailyTasksManager;
    [SerializeField] private DailyTaskWidget dailyTaskWidgetPrefab;

    [Header("UI")]
    [SerializeField] private GameObject content;
    [SerializeField] private LayoutGroup tasksLayoutGroup;
    [SerializeField] private CustomButton showButton;
    [SerializeField] private CustomButton hideButton;
    [SerializeField] private TextLocalizer updateTasksText;

    private List<DailyTaskWidget> widgets = new();

    private bool isSubscribed = false;
    private bool areWidgetsSpawned = false;

    public bool IsShown => content.activeInHierarchy;

    public event Action OnShown;
    public event Action OnHidden;

    private void OnEnable()
    {
        showButton.OnReleased.AddListener(HandleShowButtonClicked);
        hideButton.OnReleased.AddListener(HandleHideButtonClicked);

        dailyTasksManager.SetTasksViewed(true);
        TrySubscribe();
    }

    private void OnDisable()
    {
        showButton.OnReleased.RemoveListener(HandleShowButtonClicked);
        hideButton.OnReleased.RemoveListener(HandleHideButtonClicked);

        TryUnsubscribe();
    }

    private void Update()
    {
        updateTasksText.UpdateText();
    }

    public void Show()
    {
        if (IsShown) return;

        content.SetActive(true);
        TryRemoveWidgets();
        TryCreateWidgets();
        dailyTasksManager.SetTasksViewed(true);
        InputStateManager.Instance.AddInputBlockTarget(this);

        OnShown?.Invoke();
    }

    public void Hide()
    {
        if (!IsShown) return;

        content.SetActive(false);
        InputStateManager.Instance.RemoveBlockTarget(this);

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

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        dailyTasksManager.OnTasksCreated -= OnTasksInited;

        isSubscribed = false;
    }

    private void OnTasksInited()
    {
        RemoveTaskWidgets();
        CreateTaskWidgets();
    }

    private void HandleShowButtonClicked()
    {
        Show();
    }

    private void HandleHideButtonClicked()
    {
        Hide();
    }
}