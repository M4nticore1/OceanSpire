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
    [SerializeField] private TaskDefinitions[] taskDefinitions;
    [SerializeField] private int updateTasksTimeOffset = 0;

    public long NextResetTime { get; private set; } = 0;
    public bool IsAdUpdateUsed { get; private set; } = false;
    public bool IsDailyTasksViewed { get; private set; } = false;

    private List<DailyTaskInstance> currentTasks = new();
    public IReadOnlyList<DailyTaskInstance> CurrentTasks => currentTasks.AsReadOnly();

    public event Action OnTasksCreated;
    public event Action OnTasksReset;

    public event Action<bool> OnAdUpdateUsedSetTrue;
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
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= NextResetTime) {
            ResetTasks();
            UpdateNextResetTime();

            SetAdUpdateUsedSetTrue(false);
            SetTasksViewed(false);

            OnTasksReset?.Invoke();
        }
    }

    public void Init()
    {
        var dailyTasksData = new DailyTasksData()
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
        if (data == null || data.Tasks == null || data.Tasks.Length < taskDefinitions.Length) {
            Debug.LogError($"[{nameof(DailyTasksManager)}] DailyTasksData or Tasks array is null! Creating defaults.");
            Init();
            return;
        }

        RemoveTasks();
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
        OnAdUpdateUsedSetTrue?.Invoke(value);
    }

    public void SetTasksViewed(bool value)
    {
        IsDailyTasksViewed = value;
        OnTasksViewedChanged?.Invoke(value);
    }

    public DailyTaskInstanceData[] GetRandomTasksData()
    {
        var tasksData = new DailyTaskInstanceData[taskDefinitions.Length];

        for (int i = 0; i < tasksData.Length; i++) {
            int subTasksCount = taskDefinitions[i].taskDefinitions.Length;
            var randomDef = taskDefinitions[i].taskDefinitions[UnityEngine.Random.Range(0, subTasksCount)];

            int defIndex = Array.IndexOf(dailyTasksList.DailyTaskDefinitions, randomDef);

            tasksData[i] = new DailyTaskInstanceData()
            {
                Id = defIndex,
                Progress = 0
            };
        }

        return tasksData;
    }

    public long CalculateNextResetTime()
    {
        DateTime now = DateTime.UtcNow;
        DateTime nextReset = new DateTime(now.Year, now.Month, now.Day, updateTasksTimeOffset, 0, 0, DateTimeKind.Utc);

        if (nextReset <= now) {
            nextReset = nextReset.AddDays(1);
        }

        return ((DateTimeOffset)nextReset).ToUnixTimeSeconds();
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"resetTime", TimeFormatter.SecondsToHourTimer(GetRemainingResetTime())},
        };
    }

    private void CreateTasks(DailyTaskInstanceData[] tasksData)
    {
        foreach (var data in tasksData) {
            CreateTask(data);
        }

        OnTasksCreated?.Invoke();
    }

    private void CreateTask(DailyTaskInstanceData data)
    {
        if (data == null) {
            Debug.LogError($"[{nameof(DailyTasksManager)}] DailyTaskData is not valid");
            return;
        }

        var def = dailyTasksList.GetTaskDefinition(data.Id);
        int defIndex = Array.IndexOf(dailyTasksList.DailyTaskDefinitions, def);

        var task = new DailyTaskInstance(def, defIndex, data.Progress, data.Completed);
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
        NextResetTime = seconds;
    }

    private int GetRemainingResetTime()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (int)(NextResetTime - currentTime);
    }
}