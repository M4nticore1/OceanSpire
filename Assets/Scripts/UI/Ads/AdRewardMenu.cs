using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class AdRewardMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] protected CustomButton watchButton;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextLocalizer rewardDescryption;

    [Header("Remaining Text Color")]
    [SerializeField] private float lowTimeThreshold = 10f;
    [SerializeField] private Color enoughTimeColor = Color.white;
    [SerializeField] private Color lowTimeColor = Color.white;

    protected bool isOpened { get; private set; } = false;

    protected virtual void OnEnable()
    {
        RewardedAdsManager.onRewardReceived += OnRewardReceived;
        watchButton.OnReleased.AddListener(OnWatchAdButtonClicked);
        slidePanel.OnOpened += OnOpen;
        slidePanel.OnClosed += OnClose;
    }

    protected virtual void OnDisable()
    {
        RewardedAdsManager.onRewardReceived -= OnRewardReceived;
        watchButton.OnReleased.RemoveListener(OnWatchAdButtonClicked);
        slidePanel.OnOpened -= OnOpen;
        slidePanel.OnClosed -= OnClose;
    }

    //private void Update()
    //{
    //    if (!isOpened && !slidePanel.isMoving) return;

    //    AssignProgressBarFill();
    //    AssignRemainingTime();
    //}

    protected abstract void OnButtonClicked();
    protected abstract void OnOpen();
    protected abstract void OnClose();

    public void Open()
    {
        OnOpen();
        slidePanel.Open();
        AssignImage();
        AssignDescryption();

        InputStateManager.Instance.SetGameplayInputBlocked(true);
        isOpened = true;
    }

    public void Close()
    {
        OnClose();
        slidePanel.Close();

        InputStateManager.Instance.SetGameplayInputBlocked(false);
        isOpened = false;
    }

    private void AssignImage()
    {
        RewardInstance itemReward = RewardedAdsManager.Instance.currentReward;
        if (itemReward == null) {
            Debug.Log("Current ad reward is not valid!");
            return;
        }

        rewardImage.sprite = itemReward.Definition.RewardIcon;
    }

    private void AssignDescryption()
    {
        RewardInstance itemReward = RewardedAdsManager.Instance.currentReward;
        if (itemReward == null) {
            Debug.Log("Current ad reward is not valid!");
            return;
        }

        rewardDescryption.SetLocalizationItem(itemReward.Definition.RewardDescryptionLocalization);
        rewardDescryption.SetPlaceHolderLocalization(itemReward);
        rewardDescryption.UpdateText();
    }

    //private void AssignProgressBarFill()
    //{
    //    AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
    //    if (itemReward == null) {
    //        Debug.Log("Current ad reward is not valid!");
    //        return;
    //    }

    //    float limitTime = itemReward.GetLimitTime();
    //    float remainingTime = itemReward.GetRemainingTime();
    //    float alpha = limitTime != 0f ? 1f - remainingTime / limitTime : 0f;

    //    remainingTimeProgressBar.fillAmount = alpha;
    //}

    //private void AssignRemainingTime()
    //{
    //    AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
    //    float remainingTime = itemReward.GetRemainingTime();
    //    string time = TimeFormatter.SecondsToMinuteTime((int)remainingTime);
    //    remainingTimeText.SetText(time);
    //}

    //private void AssignRemainingTimeColor()
    //{
    //    AdRewardInstance itemReward = RewardedAdsManager.instance.currentReward;
    //    float remainingTime = itemReward.GetRemainingTime();

    //    if (remainingTime <= lowTimeThreshold) {
    //        remainingTimeText.color = lowTimeColor;
    //    }
    //    else {
    //        remainingTimeText.color = enoughTimeColor;
    //    }
    //}

    private void OnWatchAdButtonClicked()
    {
        RewardedAdsManager.Instance.ShowAd();
    }

    private void OnRewardReceived(RewardInstance reward)
    {
        Close();
    }
}