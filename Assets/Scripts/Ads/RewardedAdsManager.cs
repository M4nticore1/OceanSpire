using System;
using UnityEngine;

public class RewardedAdsManager : MonoBehaviour
{
    public static RewardedAdsManager Instance;

    [SerializeField] private AdsSystem adsSystem;

    public RewardInstance currentReward { get; private set; }

    public static event Action<RewardInstance> onRewardSeted;
    public static event Action<RewardInstance> onRewardRemoved;
    public static event Action<RewardInstance> onRewardReceived;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        adsSystem.onAdCompleted += OnAdCompleted;
    }

    private void OnDisable()
    {
        adsSystem.onAdCompleted -= OnAdCompleted;
    }

    public void SetCurrentReward(AdRewardDefinition definition)
    {
        SetCurrentReward(definition.CreateInstance());
    }

    public void SetCurrentReward(RewardInstance reward)
    {
        RewardInstance lastReward = currentReward;
        currentReward = reward;

        if (currentReward != null) {
            onRewardSeted?.Invoke(currentReward);
        }
        else {
            onRewardRemoved?.Invoke(lastReward);
        }
    }

    public void RemoveCurrentReward()
    {
        RewardInstance reward = currentReward;

        currentReward = null;
        onRewardRemoved?.Invoke(reward);
    }

    public void ReceiveReward()
    {
        RewardInstance reward = currentReward;

        currentReward.RecieveReward();
        currentReward = null;

        onRewardReceived?.Invoke(reward);
    }

    public void ShowAd()
    {
        adsSystem.ShowAd();
    }

    private void OnAdCompleted()
    {
        RemoveCurrentReward();
    }
}