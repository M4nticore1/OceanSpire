using UnityEngine;

[CreateAssetMenu(fileName = "UpdateDailyTasksAdRewardDefinition", menuName = "Ads Reward Definitions/UpdateDailyTasksAdRewardDefinition")]
public class UpdateDailyTasksAdRewardDefinition : RewardDefinition
{
    public override RewardInstance CreateReward()
    {
        return new UpdateDailyTasksAdRewardInstance(this);
    }
}
