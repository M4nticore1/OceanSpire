using System;
using System.Collections;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class YandexStickyBannerAds : BannerAds
{
    private Banner banner;
    private float retryDelay = 3f;

    private Coroutine retryCoroutine;

    private void Start()
    {
        YandexAds.SetAgeRestricted(true);
    }

    private void OnDestroy()
    {
        HideAd();
        CancelInvoke();
    }

    public override void ShowAd()
    {
        HideAd();
        RequestBanner();
    }

    public override void HideAd()
    {
        if (retryCoroutine != null) {
            StopCoroutine(retryCoroutine);
            retryCoroutine = null;
        }

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

    private void RequestBanner()
    {
        try {
            BannerAdSize bannerSize = BannerAdSize.Sticky(GetScreenWidthDp());
            banner = new Banner(bannerSize, AdPosition.BottomCenter);

            banner.OnAdLoaded += HandleAdLoaded;
            banner.OnAdFailedToLoad += HandleAdFailedToLoad;
            banner.OnAdClicked += HandleAdClicked;
            banner.OnImpression += HandleImpression;

            AdRequest request = new AdRequest(AdUnitId.AdUnitId);
            banner.LoadAd(request);
        }
        catch (Exception e) {
            Debug.LogError($"Error in RequestBanner: {e.Message}");
        }
    }

    private void HandleAdLoaded(object sender, EventArgs args)
    {
        if (banner != null) {
            banner.Show();
        }
        else {
            Debug.LogError("Banner is null in HandleAdLoaded");
        }
    }

    private void HandleAdFailedToLoad(object sender, AdFailureEventArgs args)
    {
        Debug.Log($"Banner AdFailedToLoad: {args.Message}");

        if (retryCoroutine != null) {
            StopCoroutine(retryCoroutine);
        }

        retryCoroutine = StartCoroutine(RequestBannerDelay());
    }

    private void HandleLeftApplication(object sender, EventArgs args)
    {
        Debug.Log("Banner LeftApplication");
    }

    private void HandleReturnedToApplication(object sender, EventArgs args)
    {
        Debug.Log("Banner ReturnedToApplication");
    }

    private void HandleAdClicked(object sender, EventArgs args)
    {
        Debug.Log("Banner AdClicked");
    }

    private void HandleImpression(object sender, ImpressionData impressionData)
    {
        var data = impressionData == null ? "null" : impressionData.rawData;
        Debug.Log($"Banner Impression: {data}");
    }
    
    private IEnumerator RequestBannerDelay()
    {
        yield return new WaitForSeconds(retryDelay);

        if (banner == null) {
            RequestBanner();
        }

        retryCoroutine = null;
    }
}