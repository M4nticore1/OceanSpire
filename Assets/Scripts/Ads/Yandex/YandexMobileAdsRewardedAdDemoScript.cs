using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class YandexMobileAdsRewardedAdDemoScript : AdsSystem
{
    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;

    private void Awake()
    {
        SetupLoader();
        RequestRewardedAd();
        DontDestroyOnLoad(gameObject);
    }

    public override void ShowAd()
    {
        if (rewardedAd != null) {
            ShowRewardedAd();
        }
        else {
            Debug.LogWarning("Реклама не готова в памяти. Загружаем заново...");
            RequestRewardedAd(true);
        }
    }

    private void SetupLoader()
    {
        rewardedAdLoader = new RewardedAdLoader();
    }

    private async void RequestRewardedAd(bool showAfterLoad = false)
    {
        YandexAds.SetAgeRestricted(true);

        if (rewardedAd != null) {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        string adUnitId = "R-M-19483291-1"; // замените на "R-M-XXXXXX-Y" R-M-19483291-1
        try {
            rewardedAd = await rewardedAdLoader.LoadAd(new AdRequest(adUnitId));

            rewardedAd.OnAdClicked += HandleAdClicked;
            rewardedAd.OnAdShown += HandleAdShown;
            rewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
            rewardedAd.OnAdImpression += HandleImpression;
            rewardedAd.OnAdDismissed += HandleAdDismissed;
            rewardedAd.OnRewarded += HandleRewarded;

            if (showAfterLoad) {
                ShowRewardedAd();
            }
        }
        catch (AdLoadingException e) {
            Debug.Log(e);
            // Ad failed to load with {e.Message}
            // Attempting to load a new ad from catch block is strongly discouraged.
        }
    }

    private void ShowRewardedAd()
    {
        if (rewardedAd == null) {
            Debug.Log("rewardedAd is not valid");
            return;
        }

        rewardedAd.Show();
    }

    public void DestroyRewardedAd()
    {
        if (rewardedAd != null) {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }

    public void HandleAdClicked(object sender, EventArgs args)
    {
        // Called when a click is recorded for rewarded ad.
    }

    public void HandleAdShown(object sender, EventArgs args)
    {
        // Called when an ad is shown.

        HandleAdStarted();
    }

    public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        // Called when an ad failed to show.

        // Clear resources after an ad dismissed.
        DestroyRewardedAd();

        // Now you can preload the next rewarded ad.
        RequestRewardedAd();
    }

    public void HandleAdDismissed(object sender, EventArgs args)
    {
        // Called when an ad is dismissed.

        // Clear resources after an ad dismissed.
        DestroyRewardedAd();

        // Now you can preload the next rewarded ad.
        RequestRewardedAd();
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        // Called when an impression is recorded for an ad.
    }

    public void HandleRewarded(object sender, Reward args)
    {
        // Called when the user can be rewarded with {args.type} and {args.amount}.

        HandleAdCompleted();
        RequestRewardedAd();
    }
}