using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyTasksManager : MonoBehaviour, ILocalizable
{
    public static DailyTasksManager Instance { get; private set; }

    [Header("Tasks")]
    [SerializeField] private DailyTaskDefinition[] easyTaskDefinitions;
    [SerializeField] private DailyTaskDefinition[] mediumTaskDefinitions;
    [SerializeField] private DailyTaskDefinition[] hardTaskDefinitions;

    [Header("Update Tasks")]
    [SerializeField] private int updateTasksCountPerDay = 1;
    [SerializeField] private int updateTasksTimeHour = 0;

    private List<DailyTaskInstance> tasks = new();
    public IReadOnlyList<DailyTaskInstance> Tasks => tasks.AsReadOnly();

    public event Action onTasksInited;

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

    public void Init(DailyTasksDataV1 data)
    {
        if (data != null) {
            CreateTasks(data.taskIds, data.taskProgresses);
        }
        else {
            CreateRandomTask();
        }

        onTasksInited?.Invoke();
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

        DailyTasksDataV1 data = new DailyTasksDataV1(taskIds, taskProgresses);

        return data;
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"updateTime", GetUpdateTime()},
        };
    }

    private void CreateTasks(int[] tasksId, int[] tasksProgress)
    {
        for (int i = 0; i < tasksId.Length; i++) {
            DailyTaskDefinition definition = easyTaskDefinitions[tasksId[i]];
            int progress = tasksProgress[i];

            CreateTask(definition, progress);
        }
    }

    private void CreateRandomTask()
    {
        CreateTask(GetRandomTaskDefinition(easyTaskDefinitions), 0);
        CreateTask(GetRandomTaskDefinition(mediumTaskDefinitions), 0);
        CreateTask(GetRandomTaskDefinition(hardTaskDefinitions), 0);
    }

    private string GetUpdateTime()
    {
        int currentSecond = DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second;

        int minTargetSeconds = updateTasksTimeHour * 3600;
        int maxTargetSeconds = (24 + updateTasksTimeHour) * 3600;

        int targetSeconds = minTargetSeconds - currentSecond >= 0 ? minTargetSeconds : maxTargetSeconds;
        int remainingSeconds = targetSeconds - currentSecond;

        string timer = TimeFormatter.SecondsToHourTime(remainingSeconds);

        return timer;
    }

    private DailyTaskInstance CreateTask(DailyTaskDefinition definition, int progress)
    {
        DailyTaskInstance task = new DailyTaskInstance(definition, progress);
        tasks.Add(task);

        return task;
    }

    private DailyTaskDefinition GetRandomTaskDefinition(DailyTaskDefinition[] tasks)
    {
        int index = UnityEngine.Random.Range(0, tasks.Length);
        DailyTaskDefinition definion = GetDefinition(tasks, index);

        return tasks[index];
    }

    private DailyTaskDefinition GetDefinition(DailyTaskDefinition[] tasks, int index)
    {
        if (easyTaskDefinitions.Length < index || easyTaskDefinitions[index] == null) {
            return GetRandomTaskDefinition(tasks);
        }
        else {
            return easyTaskDefinitions[index];
        }
    }
}