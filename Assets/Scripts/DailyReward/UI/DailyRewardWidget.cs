using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DailyRewardWidget : UIBehaviour
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

    private RewardInstance reward;

    protected override void OnEnable()
    {
        base.OnEnable();

        DailyRewardManager.Instance.OnDailyRewardRecieved += OnBonusChestRewardRecieved;
        collectButton.OnReleased.AddListener(OnTakeButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        DailyRewardManager.Instance.OnDailyRewardRecieved -= OnBonusChestRewardRecieved;
        collectButton.OnReleased.RemoveListener(OnTakeButtonClicked);
    }

    public void Init(RewardInstance reward)
    {
        this.reward = reward;
        UpdateButtonEnabled();
        UpdateButtonText();
        UpdateRewardIcon();
        UpdateRewardName();
        UpdateRewardAmount();
    }

    private void UpdateButtonEnabled()
    {
        collectButton.SetState(DailyRewardManager.Instance.AdRewardCollected || reward.IsCollected ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void UpdateButtonText()
    {
        var freeCollected = DailyRewardManager.Instance.FreeRewardCollected;
        var received = reward.IsCollected;

        freeRewardText.SetActive(!freeCollected && !received);
        adRewardText.SetActive(freeCollected && !received);
        selectedText.SetActive(received);
    }

    private void UpdateRewardIcon()
    {
        rewardIcon.sprite = reward.Definition.RewardIcon;
    }

    private void UpdateRewardName()
    {
        rewardNameText.SetLocalizationItem(reward.Definition.RewardNameLocalization);
        rewardNameText.UpdateText();
    }

    private void UpdateRewardAmount()
    {
        if (reward is ItemRewardInstance itemReward) {
            rewardAmountText.SetText(itemReward.Amount.ToString());
            rewardAmountText.UpdateText();
        }
    }

    private void OnTakeButtonClicked()
    {
        if (DailyRewardManager.Instance.FreeRewardCollected) {
            RewardedAdsManager.Instance.SetCurrentReward(reward);
            RewardedAdsManager.Instance.ShowAd();
        }
        else {
            reward.RecieveReward();
        }
    }

    private void OnBonusChestRewardRecieved(RewardInstance reward)
    {
        UpdateButtonEnabled();
        UpdateButtonText();
    }
}