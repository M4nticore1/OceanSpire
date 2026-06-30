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

    private DailyRewardManager dailyRewardManager;
    private RewardedAdsManager rewardedAdsManager;
    private RewardInstance reward;

    protected override void Awake()
    {
        base.Awake();

        dailyRewardManager = DailyRewardManager.Instance;
        rewardedAdsManager = RewardedAdsManager.Instance;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (dailyRewardManager) {
            dailyRewardManager.OnDailyRewardRecieved += OnBonusChestRewardRecieved;
        }
        else
            Debug.Log("dailyRewardManager is not valid", this);

        collectButton.OnReleased.AddListener(OnTakeButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (dailyRewardManager) {
            dailyRewardManager.OnDailyRewardRecieved -= OnBonusChestRewardRecieved;
        }

        collectButton.OnReleased.RemoveListener(OnTakeButtonClicked);
    }

    public void Init(RewardInstance reward)
    {
        if (reward == null) {
            Debug.LogError("reward is not valid", this);
            return;
        }

        dailyRewardManager = DailyRewardManager.Instance;
        rewardedAdsManager = RewardedAdsManager.Instance;

        this.reward = reward;
        UpdateButtonEnabled();
        UpdateButtonText();
        UpdateRewardIcon();
        UpdateRewardName();
        UpdateRewardAmount();
    }

    private void UpdateButtonEnabled()
    {
        if (!dailyRewardManager) {
            Debug.Log("dailyRewardManager is not valid", this);
            return;
        }

        collectButton.SetState(dailyRewardManager.ExtraRewardCollected || reward.IsCollected ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void UpdateButtonText()
    {
        if (!dailyRewardManager) {
            Debug.Log("dailyRewardManager is not valid", this);
            return;
        }

        var freeCollected = dailyRewardManager.MainRewardCollected;
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
        rewardAmountText.SetText(reward.Amount.ToString());
        rewardAmountText.UpdateText();
    }

    private void OnTakeButtonClicked()
    {
        if (!dailyRewardManager) {
            Debug.Log("dailyRewardManager is not valid", this);
            return;
        }

        if (!rewardedAdsManager) {
            Debug.Log("rewardedAdsManager is not valid", this);
            return;
        }

        if (dailyRewardManager.MainRewardCollected) {
            rewardedAdsManager.SetReward(reward);
            rewardedAdsManager.ShowAd();
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