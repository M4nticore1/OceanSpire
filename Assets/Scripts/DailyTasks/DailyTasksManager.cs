using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TaskDefinitions
{
    public DailyTaskDefinition[] taskDefinitions;
}

public class DailyTasksManager : MonoBehaviour, ILocalizable
{
    public static DailyTasksManager Instance { get; private set; }

    [Header("Tasks")]
    [SerializeField] private TaskDefinitions[] taskDefinitions;

    [Header("Update Tasks")]
    [SerializeField] private int updateTasksTimeOffset = 0;

    public long NextRestTime { get; private set; } = 0;
    public bool IsAdUpdateUsed { get; private set; } = false;
    private bool isUpdated = false;

    private List<DailyTaskInstance> currentTasks = new();
    public IReadOnlyList<DailyTaskInstance> CurrentTasks => currentTasks.AsReadOnly();

    public event Action onTasksInited;
    public event Action<bool> onAdUpdateUsedSetTrue;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (TimeManager.GetCurrentSecond() >= NextRestTime) {
            if (!TryUpdateTasks()) return;

            UpdateNextResetTime();
            SetAdUpdateUsedSetTrue(false);
        }
        else {
            SetUpdated(false);
        }
    }

    public void Init(DailyTasksData data)
    {
        CreateTasks(data != null && data.Tasks != null ? data.Tasks : GetRandomTaskIds());
        SetNextUpdateTime(data != null ? data.NextRestTime : CalculateNextUpdateSecond());
        SetAdUpdateUsedSetTrue(data != null ? data.IsAdUpdateUsed : false);
    }

    public void UpdateTasks()
    {
        RemoveTasks();
        CreateTasks(GetRandomTaskIds());
        SetUpdated(true);
    }

    public void SetAdUpdateUsedSetTrue(bool value)
    {
        IsAdUpdateUsed = value;
        onAdUpdateUsedSetTrue?.Invoke(value);
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"updateTime", GetUpdateTime()},
        };
    }

    private void SetUpdated(bool value)
    {
        isUpdated = true;
    }

    private void CreateTasks(DailyTaskInstanceData[] taskData)
    {
        for (int i = 0; i < taskData.Length; i++) {
            CreateTask(taskData[i]);
        }

        onTasksInited?.Invoke();
    }

    private void CreateTask(DailyTaskInstanceData data)
    {
        DailyTaskDefinition definition = DailyTasksList.Instance.GetTaskDefinition(data.Id);
        DailyTaskInstance task = new DailyTaskInstance(definition, data.Progress, data.IsCompleted);

        currentTasks.Add(task);
    }

    private void RemoveTasks()
    {
        for (int i = currentTasks.Count - 1; i >= 0; i--) {
            DailyTaskInstance task = currentTasks[i];
            task.RemoveTask();
            currentTasks.RemoveAt(i);
        }
    }

    private void UpdateNextResetTime()
    {
        SetNextUpdateTime(CalculateNextUpdateSecond());
    }

    private void SetNextUpdateTime(long seconds)
    {
        NextRestTime = seconds;
    }

    private bool TryUpdateTasks()
    {
        if (isUpdated) return false;

        UpdateTasks();
        return true;
    }

    private string GetUpdateTime()
    {
        int remainingSeconds = (int)(CalculateNextUpdateSecond() - TimeManager.GetCurrentSecond());

        string timer = TimeFormatter.SecondsToHourTime(remainingSeconds);

        return timer;
    }

    private long CalculateNextUpdateSecond()
    {
        long minTargetSecond = updateTasksTimeOffset * 3600;
        long maxTargetSecond = (24 + updateTasksTimeOffset) * 3600;
        long targetSecond = minTargetSecond - TimeManager.GetCurrentSecond() >= 0 ? minTargetSecond : maxTargetSecond;

        return targetSecond;
    }

    private DailyTaskInstanceData[] GetRandomTaskIds()
    {
        DailyTaskInstanceData[] tasksData = new DailyTaskInstanceData[taskDefinitions.Length];

        for (int i = 0; i < tasksData.Length; i++) {
            tasksData[i] = new DailyTaskInstanceData()
            {
                Id = UnityEngine.Random.Range(0, taskDefinitions[i].taskDefinitions.Length),
                Progress = 0
            };
        }

        return tasksData;
    }

    private DailyTaskDefinition GetRandomTaskDefinition(DailyTaskDefinition[] tasks)
    {
        int index = UnityEngine.Random.Range(0, tasks.Length);
        DailyTaskDefinition definion = GetDefinition(tasks, index);

        return tasks[index];
    }

    private DailyTaskDefinition GetDefinition(DailyTaskDefinition[] tasks, int index)
    {
        if (taskDefinitions.Length < index || taskDefinitions[index].taskDefinitions == null) {
            return GetRandomTaskDefinition(tasks);
        }
        else {
            return tasks[index];
        }
    }
}