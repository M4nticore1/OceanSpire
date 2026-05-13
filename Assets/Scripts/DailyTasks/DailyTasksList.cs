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
            }

            return instance;
        }
    }

    [SerializeField] private DailyTaskDefinition[] dailyTaskDefinitions;

    private Dictionary<int, DailyTaskDefinition> dailyTaskDefinitionsDict;

    private Dictionary<int, DailyTaskDefinition> DailyTaskDefinitionsDict
    {
        get
        {
            if (dailyTaskDefinitionsDict == null) {
                dailyTaskDefinitionsDict = new();

                foreach (var task in dailyTaskDefinitions) {
                    dailyTaskDefinitionsDict.Add((int)task.TaskId, task);
                }
            }

            return dailyTaskDefinitionsDict;
        }
    }

    public DailyTaskDefinition GetTaskDefinition(int id)
    {
        DailyTaskDefinition task = null;
        DailyTaskDefinitionsDict.TryGetValue(id, out task);

        return task;
    }
}
