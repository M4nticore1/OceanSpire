using System;
using UnityEngine;

public class RewardedAdsManager : MonoBehaviour
{
    public static RewardedAdsManager instance;

    [SerializeField] private AdsManager adsManager;
    public AdsManager AdsManager => adsManager;

    public AdRewardInstance currentReward { get; private set; }
    public static event Action onRewardChanged;

    private void Awake()
    {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        adsManager.onAdHidden += OnAdHidden;
    }

    private void OnDisable()
    {
        adsManager.onAdHidden -= OnAdHidden;
    }

    public void SetCurrentReward(AdRewardInstance reward)
    {
        currentReward = reward;
        onRewardChanged?.Invoke();
    }

    public void RemoveCurrentReward()
    {
        currentReward = null;
    }

    public void ReceiveReward()
    {
        currentReward.RecieveReward();
        currentReward = null;
    }

    private void OnAdHidden()
    {
        RemoveCurrentReward();
    }
}