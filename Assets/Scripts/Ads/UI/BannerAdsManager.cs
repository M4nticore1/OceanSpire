using System.Linq;
using UnityEngine;

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
                GameObject brokenObj = showBannerTargets[i] ? showBannerTargets[i].gameObject : null;
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

        if (!bannerAds) return;

        if (activeWindowsCount == 1) {
            bannerAds.ShowAd();
        }
    }

    private void OnHidden()
    {
        activeWindowsCount--;
        activeWindowsCount = Mathf.Clamp(activeWindowsCount, 0, activeWindowsCount);

        if (!bannerAds) return;

        if (activeWindowsCount == 0) {
            bannerAds.HideAd();
        }
    }
}