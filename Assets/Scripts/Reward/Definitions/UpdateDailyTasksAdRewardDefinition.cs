using UnityEngine;

[CreateAssetMenu(fileName = "UpdateDailyTasksAdRewardDefinition", menuName = "Ads Reward Definitions/UpdateDailyTasksAdRewardDefinition")]
public class UpdateDailyTasksAdRewardDefinition : AdRewardDefinition
{
    public override RewardInstance CreateInstance()
    {
        return new UpdateDailyTasksAdRewardInstance(this);
    }
}
