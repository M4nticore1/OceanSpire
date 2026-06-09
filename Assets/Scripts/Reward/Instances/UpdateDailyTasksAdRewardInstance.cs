using System.Collections.Generic;
using UnityEngine;

public class UpdateDailyTasksAdRewardInstance : RewardInstance
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
        base.OnRewardRecieved();

        DailyTasksManager.Instance.ResetTasks();
        DailyTasksManager.Instance.SetAdUpdateUsedSetTrue(true);
    }
}