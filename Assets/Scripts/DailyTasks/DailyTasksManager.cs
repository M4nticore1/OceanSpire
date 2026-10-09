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

    [Header("Check")]
    [SerializeField] private List<DailyTaskInstance> currentTasks = new();
    public IReadOnlyList<DailyTaskInstance> CurrentTasks => currentTasks;

    [SerializeField] private Dictionary<DailyTaskDefinition, DailyTaskInstance> currentTasksDict = new();
    public IReadOnlyDictionary<DailyTaskDefinition, DailyTaskInstance> CurrentTasksDict => currentTasksDict;

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
        if (data == null || data.Tasks == null) {
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

    public DailyTaskInstanceData GetRandomTaskData(int taskGroupIndex)
    {
        if (taskGroupIndex < 0) {
            Debug.LogError($"TaskGroupIndex is less than 0 ({taskGroupIndex})!");
            return null;
        }
        if (taskGroupIndex > taskDefinitions.Length - 1) {
            Debug.LogError($"TaskGroupIndex is greater than TaskGroups ({taskGroupIndex})!");
            return null;
        }

        var subTasksCount = taskDefinitions[taskGroupIndex].taskDefinitions.Length;
        var randomIndex = UnityEngine.Random.Range(0, subTasksCount);
        var randomDef = taskDefinitions[taskGroupIndex].taskDefinitions[randomIndex];
        var defIndex = dailyTasksList.IndexOf(randomDef);

        if (defIndex == null) {
            Debug.LogError($"DefIndex is not valid with {randomDef}!");
            return null;
        }

        return new DailyTaskInstanceData()
        {
            Id = defIndex.Value,
            Progress = 0
        };
    }

    public DailyTaskInstanceData[] GetRandomTasksData()
    {
        var tasksData = new DailyTaskInstanceData[taskDefinitions.Length];

        for (int i = 0; i < tasksData.Length; i++) {
            var taskInstance = GetRandomTaskData(i);

            tasksData[i] = taskInstance;
        }

        return tasksData;
    }

    public long CalculateNextResetTime()
    {
        var now = DateTime.UtcNow;
        var nextReset = new DateTime(now.Year, now.Month, now.Day, updateTasksTimeOffset, 0, 0, DateTimeKind.Utc);

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

    private void CreateTasks(DailyTaskInstanceData[] tasks)
    {
        if (tasks == null) {
            Debug.LogError("Tasks is not valid!");
            return;
        }
        if (tasks.Length == 0) {
            Debug.LogError("Tasks Length is 0!");
            return;
        }

        for (int i = 0; i < tasks.Length; i++) {
            var task = tasks[i];
            if (task == null) continue;

            CreateTask(task);
        }

        OnTasksCreated?.Invoke();
    }

    private void CreateTask(DailyTaskInstanceData data)
    {
        if (data == null) {
            Debug.LogError($"[{nameof(DailyTasksManager)}] Daily Task Data is not valid");
            return;
        }

        if (!dailyTasksList.TryGetTaskDefinition(data.Id, out var def)) {
            CreateTask(GetRandomTaskData(currentTasks.Count));
            return;
        }

        var reward = def.GetRandomReward();
        if (reward == null) return;

        var task = def.CreateInstance(reward);
        if (task == null) return;

        currentTasks.Add(task);
        currentTasksDict.Add(def, task);
    }

    private void RemoveTasks()
    {
        currentTasks.Clear();
        currentTasksDict.Clear();
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