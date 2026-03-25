using UnityEngine;

public class RewardedAdsMenu : MonoBehaviour
{
    [SerializeField] private AppLovinMaxAdsManager appLovinMaxAdsManager;
    [SerializeField] private RewardedAdsManager rewardedAdsManager;
    [SerializeField] private CustomButton openButton;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton watchAdButton;

    private void OnEnable()
    {
        openButton.onReleased += OnOpenButtonClicked;
        watchAdButton.onReleased += OnShowAdButtonClicked;
    }

    private void OnDisable()
    {
        openButton.onReleased -= OnOpenButtonClicked;
        watchAdButton.onReleased -= OnShowAdButtonClicked;
    }

    private void Open()
    {
        slidePanel.Open();
    }

    private void Close()
    {
        slidePanel.Close();
    }

    private void OnOpenButtonClicked()
    {
        Open();
    }

    private void OnShowAdButtonClicked()
    {
        appLovinMaxAdsManager.ShowRewardedAd();
        Close();
    }
}
