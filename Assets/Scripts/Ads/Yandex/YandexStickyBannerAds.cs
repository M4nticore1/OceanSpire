using System;
using System.Collections;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class YandexStickyBannerAds : BannerAds
{
    private Banner banner;
    private float retryDelay = 3f;

    private AdPosition lastAdPosition = AdPosition.BottomCenter;
    private Coroutine retryCoroutine;
    private bool isAdEnabled = false;

    private void Start()
    {
        YandexAds.SetAgeRestricted(true);
    }

    private void OnDestroy()
    {
        isAdEnabled = false;
        CleanUpBanner();
        if (retryCoroutine != null) StopCoroutine(retryCoroutine);
    }

    public override void ShowAd()
    {
        ShowAd(AdPosition.BottomCenter);
    }

    public override void ShowAd(AdPosition adPosition)
    {
        isAdEnabled = true;

        if (retryCoroutine != null) {
            StopCoroutine(retryCoroutine);
            retryCoroutine = null;
        }

        HideAd();
        RequestBanner(adPosition);
    }

    public override void HideAd()
    {
        isAdEnabled = false;

        if (retryCoroutine != null) {
            StopCoroutine(retryCoroutine);
            retryCoroutine = null;
        }

        CleanUpBanner();
    }

    private void CleanUpBanner()
    {
        if (banner == null) return;

        banner.OnAdLoaded -= HandleAdLoaded;
        banner.OnAdFailedToLoad -= HandleAdFailedToLoad;
        banner.OnAdClicked -= HandleAdClicked;
        banner.OnImpression -= HandleImpression;

        banner.Destroy();
        banner = null;
    }

    private int GetScreenWidthDp()
    {
        int screenWidth = (int)Screen.safeArea.width;
        return ScreenUtils.ConvertPixelsToDp(screenWidth);
    }

    private void RequestBanner(AdPosition adPosition)
    {
        if (!isAdEnabled) return;

        try {
            var bannerSize = BannerAdSize.Sticky(GetScreenWidthDp());
            banner = new Banner(bannerSize, adPosition);
            lastAdPosition = adPosition;

            banner.OnAdLoaded += HandleAdLoaded;
            banner.OnAdFailedToLoad += HandleAdFailedToLoad;
            banner.OnAdClicked += HandleAdClicked;
            banner.OnImpression += HandleImpression;

            var request = new AdRequest(AdUnitId.AdUnitId);
            banner.LoadAd(request);
        }
        catch (Exception e) {
            Debug.LogError($"Error in RequestBanner: {e.Message}");
        }
    }

    private void HandleAdLoaded(object sender, EventArgs args)
    {
        if (!isAdEnabled) {
            CleanUpBanner();
            return;
        }

        if (banner != null) {
            banner.Show();
        }
    }

    private void HandleAdFailedToLoad(object sender, AdFailureEventArgs args)
    {
        Debug.Log($"Banner AdFailedToLoad: {args.Message}");

        if (!isAdEnabled) return;

        if (retryCoroutine != null) {
            StopCoroutine(retryCoroutine);
        }

        retryCoroutine = StartCoroutine(RequestBannerDelay(lastAdPosition));
    }

    private IEnumerator RequestBannerDelay(AdPosition adPosition)
    {
        yield return new WaitForSeconds(retryDelay);

        if (isAdEnabled && banner == null) {
            RequestBanner(adPosition);
        }

        retryCoroutine = null;
    }

    private void HandleAdClicked(object sender, EventArgs args)
    {
        Debug.Log("Banner AdClicked");
    }

    private void HandleImpression(object sender, ImpressionData impressionData)
    {
        Debug.Log("Banner Impression");
    }
}