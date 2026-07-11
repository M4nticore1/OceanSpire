using System.Collections.Generic;
using UnityEngine;

public class UpdateDailyTasksAdRewardInstance : RewardInstance
{
    public UpdateDailyTasksAdRewardInstance()
    {

    }

    public UpdateDailyTasksAdRewardInstance(UpdateDailyTasksAdRewardDefinition definition) : base(definition, 0)
    {

    }

    protected override void OnRewardRecieved()
    {
        base.OnRewardRecieved();

        DailyTasksManager.Instance.ResetTasks();
        DailyTasksManager.Instance.SetAdUpdateUsedSetTrue(true);
    }
}