using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyTasksManagerMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private DailyTaskWidget dailyTaskWidgetPrefab;
    [SerializeField] private LayoutGroup tasksLayoutGroup;
    [SerializeField] private TextLocalizer updateTasksText;

    private List<DailyTaskWidget> widgets = new();

    private bool isSubscribed = false;
    private bool areWidgetsSpawned = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Update()
    {
        updateTasksText.UpdateText();
    }

    public void Open()
    {
        gameObject.SetActive(true);

        TryRemoveWidgets();
        TryCreateWidgets();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void TryCreateWidgets()
    {
        if (areWidgetsSpawned) return;

        CreateTaskWidgets();
    }

    private void CreateTaskWidgets()
    {
        for (int i = 0; i < DailyTasksManager.Instance.Tasks.Count; i++) {
            CreateTaskWidget(DailyTasksManager.Instance.Tasks[i]);
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

        DailyTasksManager.Instance.onTasksInited += OnTasksInited;
        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        DailyTasksManager.Instance.onTasksInited -= OnTasksInited;
        isSubscribed = false;
    }

    private void OnTasksInited()
    {
        RemoveTaskWidgets();
        CreateTaskWidgets();
    }
}