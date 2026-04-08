using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusRewardMenu : MonoBehaviour, IOpenable
{
    [Header("Managers")]
    [SerializeField] private AppLovinMaxRewardedAdsManager appLovinMaxAdsManager;

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
        if (RewardedAdsManager.instance.currentReward == null) return;

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
        ItemAdRewardInstance itemReward = RewardedAdsManager.instance.currentReward as ItemAdRewardInstance;
        rewardImage.sprite = itemReward.rewardData.RewardIcon;
    }

    private void AssignDescryption()
    {
        ItemAdRewardInstance itemReward = RewardedAdsManager.instance.currentReward as ItemAdRewardInstance;
        rewardDescryption.SetLocalizationItem(itemReward.rewardData.RewardDescryptionLocalization);
        rewardDescryption.SetPlaceHolderLocalization(itemReward);
        rewardDescryption.UpdateText();
    }

    private void AssignProgressBarFill()
    {
        ItemAdRewardInstance itemReward = RewardedAdsManager.instance.currentReward as ItemAdRewardInstance;
        float limitTime = itemReward.limitTime;
        float remainingTime = itemReward.remainingTime;
        float alpha = 1f - (remainingTime / limitTime);

        remainingTimeProgressBar.fillAmount = alpha;
    }

    private void AssignRemainingTime()
    {
        ItemAdRewardInstance itemReward = RewardedAdsManager.instance.currentReward as ItemAdRewardInstance;
        float remainingTime = itemReward.remainingTime;
        remainingTimeText.SetText(remainingTime.ToString("F1"));
    }

    private void AssignRemainingTimeColor()
    {
        ItemAdRewardInstance itemReward = RewardedAdsManager.instance.currentReward as ItemAdRewardInstance;
        float remainingTime = itemReward.remainingTime;

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

        ItemAdRewardInstance itemReward = RewardedAdsManager.instance.currentReward as ItemAdRewardInstance;
        float remainingTime = itemReward.remainingTime;

        if (remainingTime > 0f) return;

        Close();
    }

    private void OnShowAdButtonClicked()
    {
        appLovinMaxAdsManager.ShowAd();
        Close();
    }
}