using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyTasksList", menuName = "Lists/Daily Tasks List")]
public class DailyTasksList : ScriptableObject
{
    private static DailyTasksList instance;
    public static DailyTasksList Instance
    {
        get
        {
            if (instance == null) {
                instance = Resources.Load<DailyTasksList>("Lists/DailyTasksList");
                instance.Init();
            }
            return instance;
        }
    }

    [SerializeField] private DailyTaskDefinition[] dailyTaskDefinitions;
    private Dictionary<int, DailyTaskDefinition> dailyTaskDefinitionsDict = new();

    private void Init()
    {
        foreach (var task in dailyTaskDefinitions) {
            dailyTaskDefinitionsDict.Add((int)task.TaskId, task);
        }
    }

    public DailyTaskDefinition GetTaskDefinition(int id)
    {
        DailyTaskDefinition task = null;
        dailyTaskDefinitionsDict.TryGetValue(id, out task);

        return task;
    }
}
