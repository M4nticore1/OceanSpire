using System;
using UnityEngine;

public class RewardedAdsManager : MonoBehaviour
{
    public static RewardedAdsManager Instance;

    [SerializeField] private AdsSystem adsSystem;

    public RewardInstance CurrentReward { get; private set; }

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
        if (!definition) {
            Debug.LogError($"[{nameof(RewardedAdsManager)}] Reward Definition is not valid!");
            return;
        }

        SetReward(definition.CreateReward());
    }

    public void SetReward(RewardInstance reward)
    {
        if (reward == null) {
            Debug.LogError($"[{nameof(RewardedAdsManager)}] Reward is not valid to set current reward!");
            return;
        }

        var lastReward = CurrentReward;
        CurrentReward = reward;

        if (CurrentReward != null) {
            onRewardSeted?.Invoke(CurrentReward);
        }
        else {
            OnRewardRemoved?.Invoke(lastReward);
        }
    }

    public void RemoveReward()
    {
        if (CurrentReward == null) return;

        var reward = CurrentReward;
        CurrentReward = null;

        OnRewardRemoved?.Invoke(reward);
    }

    public void ReceiveReward()
    {
        if (CurrentReward == null) {
            Debug.LogError($"[{nameof(RewardedAdsManager)}] Current Reward is not valid to receive");
            return;
        }

        var reward = CurrentReward;

        CurrentReward.RecieveReward();
        CurrentReward = null;

        OnRewardReceived?.Invoke(reward);
    }

    public void ShowAd()
    {
        if (CurrentReward == null) {
            Debug.LogError($"[{nameof(RewardedAdsManager)}] Current Reward is not valid!");
            return;
        }

        if (!adsSystem) {
            Debug.LogError($"[{nameof(RewardedAdsManager)}] Ads System is not assigned in an inspector!");
            return;
        }

        if (!ShouldShowAd()) return;

        adsSystem.ShowAd();
    }

    private void OnAdCompleted()
    {
        RemoveReward();
    }

    private bool ShouldShowAd()
    {
        if (CurrentReward == null) return false;

        return true;
    }
}