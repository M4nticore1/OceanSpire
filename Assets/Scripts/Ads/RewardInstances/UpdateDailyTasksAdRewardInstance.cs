using System.Collections.Generic;
using UnityEngine;

public class UpdateDailyTasksAdRewardInstance : AdRewardInstance
{
    public UpdateDailyTasksAdRewardInstance(UpdateDailyTasksAdRewardDefinition definition) : base(definition)
    {

    }

    public override Dictionary<string, string> GetLocalization()
    {
        return null;
    }

    protected override void OnRewardRecieved()
    {
        DailyTasksManager.Instance.UpdateTasks();
        DailyTasksManager.Instance.SetAdUpdateUsedSetTrue(true);
    }
}