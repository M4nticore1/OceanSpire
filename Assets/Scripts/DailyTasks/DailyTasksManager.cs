using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyTasksManager : MonoBehaviour
{
    [SerializeField] private DailyTaskDefinition[] dailyTaskDefinitions;
    [SerializeField] private int tasksCountPerDay = 3;

    private List<DailyTaskInstance> tasks = new();

    public static event Action onTasksInited;

    public void Init(DailyTasksDataV1 data)
    {
        if (data != null) {
            CreateTasks(data.taskIds, data.taskProgresses);
        }
        else {
            CreateRandomTasks();
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
            taskProgresses[i] = task.progress;
        }

        DailyTasksDataV1 data = new DailyTasksDataV1(taskIds, taskProgresses);

        return data;
    }

    private void CreateTasks(int[] tasksId, int[] tasksProgress)
    {
        for (int i = 0; i < tasksId.Length; i++) {
            DailyTaskDefinition definition = dailyTaskDefinitions[tasksId[i]];
            int progress = tasksProgress[i];

            CreateTask(definition, progress);
        }
    }

    private void CreateRandomTasks()
    {
        for (int i = 0; i < tasksCountPerDay; i++) {
            DailyTaskInstance task = CreateTask(GetRandomTaskDefinition(), 0);

            while (tasks.Select(t => t.definition).Contains(task.definition)) {
                task = CreateTask(GetRandomTaskDefinition(), 0);
            }
        }
    }

    private DailyTaskInstance CreateTask(DailyTaskDefinition definition, int progress)
    {
        DailyTaskInstance task = new DailyTaskInstance(definition, progress);
        tasks.Add(task);

        return task;
    }

    private DailyTaskDefinition GetRandomTaskDefinition()
    {
        int index = UnityEngine.Random.Range(0, dailyTaskDefinitions.Length);
        DailyTaskDefinition definion = GetDefinition(index);

        return dailyTaskDefinitions[index];
    }

    private DailyTaskDefinition GetDefinition(int index)
    {
        if (dailyTaskDefinitions.Length < index || dailyTaskDefinitions[index] == null) {
            return GetRandomTaskDefinition();
        }
        else {
            return dailyTaskDefinitions[index];
        }
    }
}