using UnityEngine;

[CreateAssetMenu(fileName = "ReviveRewardDefinition", menuName = "Ads Reward Definitions/reward_revive")]
public class ReviveAdRewardDefinition : AdRewardDefinition
{
    public override AdRewardInstance CreateInstance()
    {
        return new ReviveAdRewardInstance(this, null);
    }
}
