using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyTasksMenu : MonoBehaviour
{
    [SerializeField] private DailyTaskWidget dailyTaskWidgetPrefab;
    [SerializeField] private LayoutGroup tasksLayoutGroup;

    private List<DailyTaskWidget> widgets = new();

    private bool isSubscribed = false;

    private void Awake()
    {
        TrySubscribe();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void CreateTaskWidget()
    {
        DailyTaskWidget widget = DailyTaskWidgetFactory.CreateWidget(dailyTaskWidgetPrefab, tasksLayoutGroup.transform);
        widgets.Add(widget);
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        DailyTasksManager.onTasksInited += OnTasksInited;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        DailyTasksManager.onTasksInited -= OnTasksInited;

        isSubscribed = false;
    }

    private void OnTasksInited()
    {

    }
}