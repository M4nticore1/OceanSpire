using UnityEngine;
using UnityEngine.UI;

public class DailyRewardWidget : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private CustomButton collectButton;
    [SerializeField] private GameObject freeRewardText;
    [SerializeField] private GameObject adRewardText;
    [SerializeField] private GameObject selectedText;

    [Header("Widget")]
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TextLocalizer rewardNameText;
    [SerializeField] private TextLocalizer rewardAmountText;

    private DailyRewardManager dailyRewardManager => DailyRewardManager.Instance;
    private RewardedAdsManager rewardedAdsManager => RewardedAdsManager.Instance;
    private RewardInstance reward;

    private void OnEnable()
    {
        if (dailyRewardManager != null) {
            dailyRewardManager.OnDailyRewardRecieved += HandleBonusChestRewardRecieved;
        }
        else {
            Debug.LogError($"[{nameof(DailyRewardWidget)}] DailyRewardManager is not valid");
        }

        collectButton.OnReleased.AddListener(HandleTakeButtonClicked);

        UpdateButtonEnabled();
        UpdateButtonText();
        UpdateRewardAmount();
    }

    private void OnDisable()
    {
        if (dailyRewardManager != null) {
            dailyRewardManager.OnDailyRewardRecieved -= HandleBonusChestRewardRecieved;
        }

        collectButton.OnReleased.RemoveListener(HandleTakeButtonClicked);
    }

    public void Init(RewardInstance reward)
    {
        if (reward == null) {
            Debug.LogError($"[{nameof(DailyRewardWidget)}] Reward is not valid!");
            return;
        }

        this.reward = reward;
        UpdateButtonEnabled();
        UpdateButtonText();
        UpdateRewardIcon();
        UpdateRewardName();
        UpdateRewardAmount();
    }

    private void UpdateButtonEnabled()
    {
        if (dailyRewardManager == null) return;
        if (reward == null) return;

        collectButton.SetState(dailyRewardManager.ExtraRewardCollected || reward.IsCollected ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void UpdateButtonText()
    {
        if (dailyRewardManager == null) return;
        if (reward == null) return;

        var freeCollected = dailyRewardManager.MainRewardCollected;
        var received = reward.IsCollected;

        freeRewardText.SetActive(!freeCollected && !received);
        adRewardText.SetActive(freeCollected && !received);
        selectedText.SetActive(received);
    }

    private void UpdateRewardIcon()
    {
        if (reward == null) return;

        rewardIcon.sprite = reward.Definition.RewardIcon;
    }

    private void UpdateRewardName()
    {
        if (reward == null) return;

        rewardNameText.SetLocalizationItem(reward.Definition.RewardNameLocalization);
    }

    private void UpdateRewardAmount()
    {
        if (reward == null) return;

        rewardAmountText.SetText(reward.Amount.ToString());
    }

    private void HandleTakeButtonClicked()
    {
        if (dailyRewardManager == null) {
            Debug.LogError($"[{nameof(DailyRewardWidget)}] Daily Reward Manager is not valid!");
            return;
        }

        if (rewardedAdsManager == null) {
            Debug.LogError($"[{nameof(DailyRewardWidget)}] Rewarded Ads Manager is not valid!");
            return;
        }

        if (reward == null) {
            Debug.LogError($"[{nameof(DailyRewardWidget)}] Daily Reward is not valid!");
            return;
        }

        if (dailyRewardManager.MainRewardCollected) {
            rewardedAdsManager.SetReward(reward);
            rewardedAdsManager.ShowAd();
        }
        else {
            Debug.Log(reward.Amount);
            reward.RecieveReward();
        }

        UpdateButtonEnabled();
        UpdateButtonText();
    }

    private void HandleBonusChestRewardRecieved(RewardInstance reward)
    {
        UpdateButtonEnabled();
        UpdateButtonText();
    }
}