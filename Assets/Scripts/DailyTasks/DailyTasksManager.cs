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
    public bool IsDailyTasksViewed { get; private set; } = false;
    private bool isUpdated = false;

    private List<DailyTaskInstance> currentTasks = new();
    public IReadOnlyList<DailyTaskInstance> CurrentTasks => currentTasks.AsReadOnly();

    public event Action OnTasksInited;
    public event Action OnTasksReset;
    public event Action<bool> onAdUpdateUsedSetTrue;
    public event Action<bool> OnTasksViewedChanged;

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
            if (!TryResetTasks()) return;

            UpdateNextResetTime();
            SetAdUpdateUsedSetTrue(false);
            SetUpdated(true);
            SetTasksViewed(false);
            OnTasksReset?.Invoke();
        }
        else {
            SetUpdated(false);
        }
    }

    public void Init()
    {
        DailyTasksData dailyTasksData = new DailyTasksData()
        {
            Tasks = GetRandomTasksData(),
            NextResetTime = CalculateNextResetTime(),
            AdUpdateUsed = false,
            TasksViewed = false,
        };

        Init(dailyTasksData);
    }

    public void Init(DailyTasksData data)
    {
        CreateTasks(data.Tasks);
        SetNextUpdateTime(data.NextResetTime);
        SetAdUpdateUsedSetTrue(data.AdUpdateUsed);
        SetTasksViewed(data.TasksViewed);
    }

    public void ResetTasks()
    {
        RemoveTasks();
        CreateTasks(GetRandomTasksData());
    }

    public void SetAdUpdateUsedSetTrue(bool value)
    {
        IsAdUpdateUsed = value;
        onAdUpdateUsedSetTrue?.Invoke(value);
    }

    public void SetTasksViewed(bool value)
    {
        IsDailyTasksViewed = value;
        OnTasksViewedChanged?.Invoke(value);
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

        OnTasksInited?.Invoke();
    }

    private void CreateTask(DailyTaskInstanceData data)
    {
        if (data == null) {
            Debug.LogError("DailyTaskData is not valid");
            return;
        }

        var def = dailyTasksList.GetTaskDefinition(data.Id);
        var task = new DailyTaskInstance(def, data.Progress, data.Completed);

        currentTasks.Add(task);
    }

    private void RemoveTasks()
    {
        for (int i = currentTasks.Count - 1; i >= 0; i--) {
            var task = currentTasks[i];
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

    private bool TryResetTasks()
    {
        if (isUpdated) return false;

        ResetTasks();
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
        var tasksData = new DailyTaskInstanceData[taskDefinitions.Length];

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
        var definion = GetDefinition(tasks, index);

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