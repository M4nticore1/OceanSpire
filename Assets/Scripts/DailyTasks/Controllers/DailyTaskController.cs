using System;
using UnityEngine;

public abstract class DailyTaskController : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private DailyTasksManager dailyTasksManager;

    [SerializeField] private DailyTaskDefinition dailyTaskDefinition;
    public DailyTaskDefinition DailyTaskDefinition => dailyTaskDefinition;

    protected DailyTaskInstance taskInstance;

    private bool isSubscribed = false;

    private void Awake()
    {
        if (dailyTasksManager == null) {
            Debug.LogError($"DailyTasksManager is not valid at {this}!");
        }
        if (dailyTaskDefinition == null) {
            Debug.LogError($"DailyTaskDefinition is not valid at {this}!");
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        TrySubscribe();

        if (dailyTasksManager != null) {
            dailyTasksManager.CurrentTasksDict.TryGetValue(dailyTaskDefinition, out var task);
            taskInstance = task;
        }
    }

    protected virtual bool Subscribe()
    {
        if (dailyTasksManager != null) {
            dailyTasksManager.OnTasksCreated += HandleTasksCreated;
        }

        return true;
    }

    protected virtual bool Unsubscribe()
    {
        if (dailyTasksManager != null) {
            dailyTasksManager.OnTasksCreated -= HandleTasksCreated;
        }

        return true;
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!Subscribe()) return;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!Unsubscribe()) return;

        isSubscribed = false;
    }

    protected void AddTaskProgress(int value)
    {
        if (taskInstance != null) {
            taskInstance.AddProgress(value);
        }
    }

    private void HandleTasksCreated()
    {
        dailyTasksManager.CurrentTasksDict.TryGetValue(dailyTaskDefinition, out var task);
        taskInstance = task;
    }
}
