using System;
using System.Linq;
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
    public DailyTaskDefinition[] DailyTaskDefinitions => dailyTaskDefinitions;

    public bool TryGetTaskDefinition(int id, out DailyTaskDefinition taskDefinition)
    {
        taskDefinition = null;

        if (id < 0) {
            //Debug.LogError($"Id is less than 0 ({id})!");
            return false;
        }
        if (id >= dailyTaskDefinitions.Length) {
            //Debug.LogError($"Id {id} is greater than tasks count {dailyTaskDefinitions.Length}!");
            return false;
        }

        taskDefinition = dailyTaskDefinitions[id];
        if (taskDefinition == null) {
            Debug.LogError($"Daily Task is not valid at id {id}!");
            return false;
        }

        return true;
    }

    public int? IndexOf(DailyTaskDefinition def)
    {
        if (def == null) {
            Debug.LogError($"Daily Task Definition is not valid!");
            return null;
        }

        if (!dailyTaskDefinitions.Contains(def)) {
            Debug.LogError($"Daily Tasks does not contain {def}!");
            return null;
        }

        return Array.IndexOf(dailyTaskDefinitions, def);
    }
}