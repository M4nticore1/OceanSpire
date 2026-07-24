using System;
using System.Collections;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class YandexStickyBannerAds : BannerAds
{
    [SerializeField] private float retryDelay = 1f;
    private Banner banner;

    private AdPosition lastAdPosition = AdPosition.BottomCenter;
    private Coroutine retryCoroutine;
    private bool isAdEnabled = false;

    private void OnDestroy()
    {
        isAdEnabled = false;

        if (retryCoroutine != null) {
            StopCoroutine(retryCoroutine);
            retryCoroutine = null;
        }

        CleanUpBanner();
    }

    private void Update()
    {
        if (!isAdEnabled && banner != null) {
            banner.Hide();
            banner.Destroy();
            banner = null;
        }
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

        CleanUpBanner();
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

        try {
            banner.Hide();
            banner.Destroy();
            banner = null;
        }
        catch (Exception e) {
            Debug.LogError($"Exception during banner cleanup: {e.Message}");
        }
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

        banner?.Show();
    }

    private void HandleAdFailedToLoad(object sender, AdFailureEventArgs args)
    {
        if (!isAdEnabled) return;

        if (retryCoroutine != null) {
            StopCoroutine(retryCoroutine);
        }

        retryCoroutine = StartCoroutine(RequestBannerDelay(lastAdPosition));
    }

    private IEnumerator RequestBannerDelay(AdPosition adPosition)
    {
        yield return new WaitForSeconds(retryDelay);

        if (isAdEnabled) {
            CleanUpBanner();
            RequestBanner(adPosition);
        }

        retryCoroutine = null;
    }

    private void HandleAdClicked(object sender, EventArgs args)
    {

    }

    private void HandleImpression(object sender, ImpressionData impressionData)
    {

    }
}