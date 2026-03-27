using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardedAdsMenu : MonoBehaviour, IOpenable
{
    [Header("Managers")]
    [SerializeField] private RewardedAdsManager rewardedAdsManager;
    [SerializeField] private AppLovinMaxAdsManager appLovinMaxAdsManager;

    [Header("UI")]
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextLocalizer rewardDescryption;
    [SerializeField] private CustomButton watchAdButton;
    [SerializeField] private Image remainingTimeProgressBar;
    [SerializeField] private TextMeshProUGUI remainingTimeText;

    [Header("Remaining Text Color")]
    [SerializeField] private float lowTimeThreshold = 10f;
    [SerializeField] private Color enoughTimeColor = Color.white;
    [SerializeField] private Color lowTimeColor = Color.white;

    private bool isOpened = false;

    private void OnEnable()
    {
        watchAdButton.onReleased += OnShowAdButtonClicked;
        slidePanel.onClosed += OnClosed;
    }

    private void OnDisable()
    {
        watchAdButton.onReleased -= OnShowAdButtonClicked;
        slidePanel.onClosed -= OnClosed;
    }

    private void Update()
    {
        if (rewardedAdsManager.currentReward == null) return;

        AssignProgressBarFill();
        AssignRemainingTime();
        AssignRemainingTimeColor();
        CheckRemainingTimeToClose();
    }

    public void Open()
    {
        AssignImage();
        AssignDescryption();

        slidePanel.Open();
        InputStateManager.instance.SetGameplayInputBlocked(true);
        isOpened = true;
    }

    public void Close()
    {
        slidePanel.Close();
        OnClosed();
    }

    private void OnClosed()
    {
        InputStateManager.instance.SetGameplayInputBlocked(false);
        isOpened = false;
    }

    private void AssignImage()
    {
        rewardImage.sprite = rewardedAdsManager.currentReward.rewardData.RewardIcon;
    }

    private void AssignDescryption()
    {
        rewardDescryption.SetLocalizationItem(rewardedAdsManager.currentReward.rewardData.RewardDescryptionLocalization);
        rewardDescryption.SetPlaceHolderLocalization(rewardedAdsManager.currentReward);
        rewardDescryption.UpdateText();
    }

    private void AssignProgressBarFill()
    {
        float limitTime = rewardedAdsManager.currentReward.limitTime;
        float remainingTime = rewardedAdsManager.currentReward.remainingTime;
        float alpha = 1f - (remainingTime / limitTime);

        remainingTimeProgressBar.fillAmount = alpha;
    }

    private void AssignRemainingTime()
    {
        float remainingTime = rewardedAdsManager.currentReward.remainingTime;
        remainingTimeText.SetText(remainingTime.ToString("F1"));
    }

    private void AssignRemainingTimeColor()
    {
        float remainingTime = rewardedAdsManager.currentReward.remainingTime;

        if (remainingTime <= lowTimeThreshold) {
            remainingTimeText.color = lowTimeColor;
        }
        else {
            remainingTimeText.color = enoughTimeColor;
        }
    }

    private void CheckRemainingTimeToClose()
    {
        if (!isOpened) return;

        float remainingTime = rewardedAdsManager.currentReward.remainingTime;
        if (remainingTime > 0f) return;

        Close();
    }

    private void OnShowAdButtonClicked()
    {
        appLovinMaxAdsManager.ShowRewardedAd();
        Close();
    }
}