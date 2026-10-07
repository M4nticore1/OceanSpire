using UnityEngine;

[CreateAssetMenu(fileName = "ReviveRewardDefinition", menuName = "Ads Reward Definitions/reward_revive")]
public class ReviveAdRewardDefinition : RewardDefinition
{
    public override RewardInstance CreateReward()
    {
        return new ReviveAdRewardInstance(this, null);
    }
}
