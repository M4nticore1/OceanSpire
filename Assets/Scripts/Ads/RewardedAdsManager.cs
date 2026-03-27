using UnityEngine;

public class RewardedAdsManager : MonoBehaviour
{
    public AdRewardInstance currentReward { get; private set; }

    public void SetCurrentReward(AdRewardInstance reward)
    {
        currentReward = reward;
    }

    public void ReceiveReward()
    {
        currentReward.RecieveReward();
        SetCurrentReward(null);
    }
}