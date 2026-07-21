using System.Linq;
using UnityEngine;
using YandexMobileAds.Base;

public class BannerAdsManager : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] showBannerTargets;
    public IOpenable[] ShowBannerTargets;

    private int activeWindowsCount = 0;
    private BannerAds bannerAds => BannerAds.Instance;

    private void Awake()
    {
        ShowBannerTargets = showBannerTargets.OfType<IOpenable>().ToArray();

        if (showBannerTargets.Length != ShowBannerTargets.Length) {
            Debug.LogError($"[{nameof(BannerAdsManager)}] There are elements without IOpenable in the inspector!");
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < ShowBannerTargets.Length; i++) {
            var target = ShowBannerTargets[i];
            if (target == null) {
                var brokenObj = showBannerTargets[i] ? showBannerTargets[i].gameObject : null;
                Debug.LogError($"[{nameof(BannerAdsManager)}] An element at index {i} ({brokenObj?.name ?? "null"}) doesn't implement IOpenable!");
                continue;
            }

            target.OnShown += OnShown;
            target.OnHidden += OnHidden;
        }
    }

    private void OnDisable()
    {
        foreach (var target in ShowBannerTargets) {
            if (target == null) continue;

            target.OnShown -= OnShown;
            target.OnHidden -= OnHidden;
        }

        activeWindowsCount = 0;

        if (bannerAds) {
            bannerAds.HideAd();
        }
    }

    private void OnShown()
    {
        activeWindowsCount++;

        if (!bannerAds) {
            Debug.LogError($"[{nameof(BannerAdsManager)}] Banner Ads is not on the scene!");
            return;
        }

        if (activeWindowsCount == 1) {
            bannerAds.ShowAd(AdPosition.BottomCenter);
        }
    }

    private void OnHidden()
    {
        activeWindowsCount = Mathf.Max(0, activeWindowsCount - 1);

        if (!bannerAds) {
            Debug.LogError($"[{nameof(BannerAdsManager)}] Banner Ads is not on the scene!");
            return;
        }

        if (activeWindowsCount == 0) {
            bannerAds.HideAd();
        }
    }
}