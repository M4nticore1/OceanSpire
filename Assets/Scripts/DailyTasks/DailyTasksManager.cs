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

    [SerializeField] private DailyTasksList dailyTasksList;

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
        CreateTasks(data.Tasks);
        SetNextUpdateTime(data.NextResetTime);
        SetAdUpdateUsedSetTrue(data.AdUpdateUsed);
    }

    public void UpdateTasks()
    {
        RemoveTasks();
        CreateTasks(GetRandomTasksData());
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

    private void CreateTasks(DailyTaskInstanceData[] tasksData)
    {
        foreach (var data in tasksData) {
            CreateTask(data);
        }

        onTasksInited?.Invoke();
    }

    private void CreateTask(DailyTaskInstanceData data)
    {
        DailyTaskDefinition def = dailyTasksList.GetTaskDefinition(data.Id);
        DailyTaskInstance task = new DailyTaskInstance(def, data.Progress, data.Completed);

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
        SetNextUpdateTime(CalculateNextResetTime());
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
        int remainingSeconds = (int)(CalculateNextResetTime() - TimeManager.GetCurrentSecond());

        string timer = TimeFormatter.SecondsToHourTime(remainingSeconds);

        return timer;
    }

    public long CalculateNextResetTime()
    {
        long minTargetSecond = updateTasksTimeOffset * 3600;
        long maxTargetSecond = (24 + updateTasksTimeOffset) * 3600;
        long targetSecond = minTargetSecond - TimeManager.GetCurrentSecond() >= 0 ? minTargetSecond : maxTargetSecond;

        return targetSecond;
    }

    public DailyTaskInstanceData[] GetRandomTasksData()
    {
        DailyTaskInstanceData[] tasksData = new DailyTaskInstanceData[taskDefinitions.Length];

        for (int i = 0; i < tasksData.Length; i++) {
            tasksData[i] = new DailyTaskInstanceData()
            {
                Id = (int)taskDefinitions[i].taskDefinitions[UnityEngine.Random.Range(0, taskDefinitions[i].taskDefinitions.Length)].TaskId,
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