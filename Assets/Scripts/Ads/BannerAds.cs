using UnityEngine;
using YandexMobileAds.Base;

public abstract class BannerAds : AdsSystem
{
    public static BannerAds Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance) {
            Debug.LogError($"[{nameof(BannerAds)}] Another Banner Ads on the scene!");
            return;
        }

        Instance = this;
    }

    public abstract void ShowAd(AdPosition adPosition);
}