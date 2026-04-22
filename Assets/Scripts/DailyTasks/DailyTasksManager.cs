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
    [SerializeField] private int updateTasksCountPerDay = 1;
    [SerializeField] private int updateTasksTimeHour = 0;

    private int nextUpdateTime = 0;
    private bool isUpdated = false;
    private bool isAdUpdateUsed = false;

    private List<DailyTaskInstance> tasks = new();
    public IReadOnlyList<DailyTaskInstance> Tasks => tasks.AsReadOnly();

    public event Action onTasksInited;
    public event Action<bool> onAdUpdateUsedSetTrue;

    private void Start()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        WorldData worldData = WorldSaveManager.Instance.currentSaveWorldData;
        var data = worldData != null ? worldData.DailyTasksData : null;
        Init(data);
    }

    private void Update()
    {
        if (TimeManager.GetCurrentSecond() >= nextUpdateTime) {
            if (TryUpdateTasks()) {
                SetNextUpdateTime(CalculateNextUpdateSecond());
                SetAdUpdateUsedSetTrue(false);
            }
        }
        else {
            SetUpdated(false);
        }
    }

    public void Init(DailyTasksDataV1 data)
    {
        if (data != null) {
            CreateTasks(data.taskIds, data.taskProgresses);
            SetNextUpdateTime(data.nextUpdateTime);
            SetAdUpdateUsedSetTrue(data.adUpdateUsed);
            
        }
        else {
            CreateTasks(GetRandomTaskIds(), GetEmptyTaskProgresses());
            SetNextUpdateTime(CalculateNextUpdateSecond());
        }
    }

    public void UpdateTasks()
    {
        RemoveTasks();
        CreateTasks(GetRandomTaskIds(), GetEmptyTaskProgresses());
        SetUpdated(true);
    }

    public void SetAdUpdateUsedSetTrue(bool value)
    {
        isAdUpdateUsed = value;
        onAdUpdateUsedSetTrue?.Invoke(value);
    }

    public DailyTasksDataV1 GetCurrentData()
    {
        int[] taskIds = new int[tasks.Count];
        int[] taskProgresses = new int[tasks.Count];

        for (int i = 0; i < tasks.Count; i++) {
            DailyTaskInstance task = tasks[i];
            taskIds[i] = tasks.IndexOf(task);
            taskProgresses[i] = task.Progress;
        }

        DailyTasksDataV1 data = new DailyTasksDataV1(taskIds, taskProgresses, nextUpdateTime, isAdUpdateUsed);

        return data;
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"updateTime", GetUpdateTime()},
        };
    }

    private bool TryUpdateTasks()
    {
        if (isUpdated) return false;

        UpdateTasks();
        return true;
    }

    private void SetUpdated(bool value)
    {
        isUpdated = true;
    }

    private void CreateTasks(int[] tasksId, int[] tasksProgress)
    {
        for (int i = 0; i < tasksId.Length; i++) {
            DailyTaskDefinition definition = taskDefinitions[i].taskDefinitions[tasksId[i]];
            int progress = tasksProgress[i];

            CreateTask(definition, progress);
        }

        onTasksInited?.Invoke();
    }

    private void CreateTask(DailyTaskDefinition definition, int progress)
    {
        DailyTaskInstance task = new DailyTaskInstance(definition, progress);
        tasks.Add(task);
    }

    private void RemoveTasks()
    {
        for (int i = tasks.Count - 1; i >= 0; i--) {
            DailyTaskInstance task = tasks[i];
            task.RemoveTask();
            tasks.RemoveAt(i);
        }
    }

    private void SetNextUpdateTime(int seconds)
    {
        nextUpdateTime = seconds;
    }

    private string GetUpdateTime()
    {
        int remainingSeconds = CalculateNextUpdateSecond() - TimeManager.GetCurrentSecond();

        string timer = TimeFormatter.SecondsToHourTime(remainingSeconds);

        return timer;
    }

    private int CalculateNextUpdateSecond()
    {
        int minTargetSecond = updateTasksTimeHour * 3600;
        int maxTargetSecond = (24 + updateTasksTimeHour) * 3600;
        int targetSecond = minTargetSecond - TimeManager.GetCurrentSecond() >= 0 ? minTargetSecond : maxTargetSecond;

        return targetSecond;
    }

    private int[] GetRandomTaskIds()
    {
        int[] ids = new int[taskDefinitions.Length];

        for (int i = 0; i < ids.Length; i++) {
            ids[i] = GetRandomTaskId(taskDefinitions[i].taskDefinitions);
        }

        return ids;
    }

    private int[] GetEmptyTaskProgresses()
    {
        int[] progresses = new int[taskDefinitions.Length];

        for (int i = 0; i < progresses.Length; i++) {
            progresses[i] = 0;
        }

        return progresses;
    }

    private int GetRandomTaskId(DailyTaskDefinition[] tasks)
    {
        return UnityEngine.Random.Range(0, tasks.Length);
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