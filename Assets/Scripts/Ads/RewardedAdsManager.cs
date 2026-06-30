using System;
using UnityEngine;

public class RewardedAdsManager : MonoBehaviour
{
    public static RewardedAdsManager Instance;

    [SerializeField] private AdsSystem adsSystem;

    public RewardInstance currentReward { get; private set; }

    public static event Action<RewardInstance> onRewardSeted;
    public static event Action<RewardInstance> OnRewardRemoved;
    public static event Action<RewardInstance> OnRewardReceived;

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

    public void SetReward(AdRewardDefinition definition)
    {
        SetReward(definition.CreateReward());
    }

    public void SetReward(RewardInstance reward)
    {
        if (reward == null) {
            Debug.LogError("reward is not valid to set current reward");
            return;
        }

        var lastReward = currentReward;
        currentReward = reward;

        if (currentReward != null) {
            onRewardSeted?.Invoke(currentReward);
        }
        else {
            OnRewardRemoved?.Invoke(lastReward);
        }
    }

    public void RemoveReward()
    {
        if (currentReward == null) return;

        var reward = currentReward;
        currentReward = null;

        OnRewardRemoved?.Invoke(reward);
    }

    public void ReceiveReward()
    {
        if (currentReward == null) {
            Debug.LogError("currentReward is not valid to receive");
            return;
        }

        var reward = currentReward;

        currentReward.RecieveReward();
        currentReward = null;

        OnRewardReceived?.Invoke(reward);
    }

    public void ShowAd()
    {
        adsSystem.ShowAd();
    }

    private void OnAdCompleted()
    {
        RemoveReward();
    }
}