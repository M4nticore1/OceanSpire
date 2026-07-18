using System;
using UnityEngine;

public abstract class AdsSystem : MonoBehaviour
{
    [SerializeField] private RewardedAdsManager rewardedAdsManager;

    [SerializeField] private AdUnitIdDefinition adUnitId;
    public AdUnitIdDefinition AdUnitId => adUnitId;

    public bool isAdDisplayed { get; private set; } = false;

    public event Action OnAdShown;
    public event Action onAdCompleted;

    protected virtual void Awake()
    {

    }

    public abstract void ShowAd();
    public abstract void HideAd();

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