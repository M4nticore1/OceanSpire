using UnityEngine;

public class DailyTasksLoader : Loader
{
    protected override void Load(WorldData worldData)
    {
        var data = worldData != null ? worldData.DailyTasks : null;

        DailyTasksManager.Instance.Init(data);
    }
}
