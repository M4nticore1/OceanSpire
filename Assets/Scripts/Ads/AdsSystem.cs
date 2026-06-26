using System;
using UnityEngine;

public abstract class AdsSystem : MonoBehaviour
{
    [SerializeField] private RewardedAdsManager rewardedAdsManager;

    public bool isAdDisplayed { get; private set; } = false;

    public event Action OnAdShown;
    public event Action onAdCompleted;

    public abstract void ShowAd();

    protected void HandleAdStarted()
    {
        isAdDisplayed = true;
        OnAdShown?.Invoke();
    }

    protected void HandleAdCompleted()
    {
        isAdDisplayed = false;

        rewardedAdsManager.ReceiveReward();

        onAdCompleted?.Invoke();
    }
}