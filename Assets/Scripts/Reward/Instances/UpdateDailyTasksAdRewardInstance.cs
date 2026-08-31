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

    protected override void HandleRewardRecieved()
    {
        base.HandleRewardRecieved();

        DailyTasksManager.Instance.ResetTasks();
        DailyTasksManager.Instance.SetAdUpdateUsedSetTrue(true);
    }
}