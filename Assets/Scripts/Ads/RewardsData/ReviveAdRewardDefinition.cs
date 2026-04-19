using UnityEngine;

[CreateAssetMenu(fileName = "ReviveRewardDefinition", menuName = "Ads Reward/reward_revive")]
public class ReviveAdRewardDefinition : AdRewardDefinition
{
    public override AdRewardInstance CreateInstance(float limitTime)
    {
        return new ReviveAdRewardInstance(this, limitTime, null);
    }
}
