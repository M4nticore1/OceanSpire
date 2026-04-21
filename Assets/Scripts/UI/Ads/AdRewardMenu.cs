using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdRewardMenu : MonoBehaviour, IOpenable
{
    [Header("UI")]
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton watchButton;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextLocalizer rewardDescryption;
    [SerializeField] private Image remainingTimeProgressBar;
    [SerializeField] private TextMeshProUGUI remainingTimeText;

    [Header("Remaining Text Color")]
    [SerializeField] private float lowTimeThreshold = 10f;
    [SerializeField] private Color enoughTimeColor = Color.white;
    [SerializeField] private Color lowTimeColor = Color.white;

    private bool isOpened = false;

    protected virtual void OnEnable()
    {
        RewardedAdsManager.onRewardChanged += OnRewardChanged;
        watchButton.onReleased += OnShowAdButtonClicked;
        slidePanel.onOpened += OnOpened;
        slidePanel.onClosed += OnClosed;
    }

    protected virtual void OnDisable()
    {
        RewardedAdsManager.onRewardChanged -= OnRewardChanged;
        watchButton.onReleased -= OnShowAdButtonClicked;
        slidePanel.onOpened -= OnOpened;
        slidePanel.onClosed -= OnClosed;
    }

    private void Update()
    {
        if (!isOpened && !slidePanel.isMoving) return;

        UpdateTime();
    }

    public void Open()
    {
        AssignImage();
        AssignDescryption();

        slidePanel.Open();
    }

    public void Close()
    {
        slidePanel.Close();
    }

    protected virtual void OnOpened()
    {
        InputStateManager.instance.SetGameplayInputBlocked(true);
        isOpened = true;
    }

    protected virtual void OnClosed()
    {
        InputStateManager.instance.SetGameplayInputBlocked(false);
        isOpened = false;
    }

    private void UpdateTime()
    {
        AssignProgressBarFill();
        AssignRemainingTime();
        //AssignRemainingTimeColor();
        CheckRemainingTimeToClose();
    }

    private void AssignImage()
    {
        AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
        if (itemReward == null) {
            Debug.Log("Current ad reward is not valid!");
            return;
        }

        rewardImage.sprite = itemReward.definition.RewardIcon;
    }

    private void AssignDescryption()
    {
        AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
        if (itemReward == null) {
            Debug.Log("Current ad reward is not valid!");
            return;
        }

        rewardDescryption.SetLocalizationItem(itemReward.definition.RewardDescryptionLocalization);
        rewardDescryption.SetPlaceHolderLocalization(itemReward);
        rewardDescryption.UpdateText();
    }

    private void AssignProgressBarFill()
    {
        AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
        if (itemReward == null) {
            Debug.Log("Current ad reward is not valid!");
            return;
        }

        float limitTime = itemReward.GetLimitTime();
        float remainingTime = itemReward.GetRemainingTime();
        float alpha = limitTime != 0f ? 1f - remainingTime / limitTime : 0f;

        remainingTimeProgressBar.fillAmount = alpha;
    }

    private void AssignRemainingTime()
    {
        AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
        float remainingTime = itemReward.GetRemainingTime();
        string time = TimeFormatter.SecondsToMinuteTime((int)remainingTime);
        remainingTimeText.SetText(time);
    }

    private void AssignRemainingTimeColor()
    {
        AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
        float remainingTime = itemReward.GetRemainingTime();

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

        AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
        float remainingTime = itemReward.GetRemainingTime();

        if (remainingTime > 0f) return;

        Close();
    }

    private void OnShowAdButtonClicked()
    {
        RewardedAdsManager.instance.AdsManager.ShowAd();
        Close();
    }

    private void OnRewardChanged()
    {
        UpdateTime();
    }
}