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

    private ItemAdRewardInstance reward;

    protected override void OnEnable()
    {
        base.OnEnable();

        DailyRewardManager.Instance.onDailyRewardRecieved += OnBonusChestRewardRecieved;
        collectButton.onReleased.AddListener(OnTakeButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        DailyRewardManager.Instance.onDailyRewardRecieved -= OnBonusChestRewardRecieved;
        collectButton.onReleased.RemoveListener(OnTakeButtonClicked);
    }

    public void Init(ItemAdRewardInstance reward)
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
        collectButton.SetState(!DailyRewardManager.Instance.CanSelectReward() || reward.IsRecieved ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void UpdateButtonText()
    {
        var free = DailyRewardManager.Instance.CanSelectFreeReward();
        var received = reward.IsRecieved;

        freeRewardText.SetActive(free && !received);
        adRewardText.SetActive(!free && !received);
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
        rewardAmountText.SetText(reward.Amount.ToString());
        rewardAmountText.UpdateText();
    }

    private void OnTakeButtonClicked()
    {
        if (DailyRewardManager.Instance.CanSelectFreeReward()) {
            reward.RecieveReward();
        }
        else {
            RewardedAdsManager.Instance.SetCurrentReward(reward);
            RewardedAdsManager.Instance.ShowAd();
        }
    }

    private void OnBonusChestRewardRecieved(AdRewardInstance reward)
    {
        UpdateButtonEnabled();
        UpdateButtonText();
    }
}