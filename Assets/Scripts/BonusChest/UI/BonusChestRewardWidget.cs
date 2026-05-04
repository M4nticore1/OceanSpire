using UnityEngine;
using UnityEngine.UI;

public class BonusChestRewardWidget : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private CustomButton button;
    [SerializeField] private GameObject freeRewardButtonContent;
    [SerializeField] private GameObject adRewardButtonContent;

    [Header("Widget")]
    [SerializeField] private GameObject recievedMenu;
    [SerializeField] private Image rewardIcon;

    private AdRewardInstance reward;

    private void OnEnable()
    {
        BonusChestManager.Instance.onRewardRecieved += OnBonusChestRewardRecieved;
        button.onReleased += OnTakeButtonClicked;
    }

    private void OnDisable()
    {
        BonusChestManager.Instance.onRewardRecieved -= OnBonusChestRewardRecieved;
        button.onReleased -= OnTakeButtonClicked;
    }

    public void Init(ItemAdRewardInstance reward)
    {
        this.reward = reward;

        UpdateFreeRewardEnabled();
        UpdateRewardEnabled();
        UpdateRewardIcon();
    }

    private void UpdateFreeRewardEnabled()
    {
        var free = BonusChestManager.Instance.CanTakeFreeReward();
        freeRewardButtonContent.SetActive(free);
        adRewardButtonContent.SetActive(!free);
    }

    private void UpdateRewardEnabled()
    {
        recievedMenu.SetActive(reward.IsRecieved);
    }

    private void UpdateRewardIcon()
    {
        rewardIcon.sprite = reward.Definition.RewardIcon;
    }

    private void OnTakeButtonClicked()
    {
        if (BonusChestManager.Instance.CanTakeFreeReward()) {
            reward.RecieveReward();
        }
        else {
            RewardedAdsManager.Instance.SetCurrentReward(reward);
            RewardedAdsManager.Instance.ShowAd();
        }
    }

    private void OnBonusChestRewardRecieved(AdRewardInstance reward)
    {
        UpdateRewardEnabled();
    }
}