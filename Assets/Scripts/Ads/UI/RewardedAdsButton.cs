using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardedAdsButton : UIBehaviour
{
    [SerializeField] private RewardedAdsManager rewardedAdsManager;
    [SerializeField] private RewardedAdsButtonManager rewardedAdsButtonManager;
    [SerializeField] private BonusRewardMenu rewardedAdsMenu;
    [SerializeField] private CustomButton button;
    [SerializeField] private Image itemImage;
    [SerializeField] private Image progressBar;

    protected override void OnEnable()
    {
        base.OnEnable();

        button.onReleased += OnButtonReleased;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        button.onReleased -= OnButtonReleased;
    }

    private void Update()
    {
        if (rewardedAdsButtonManager.currentReward == null) return;

        AssignProgressBarFill();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        AssignImage();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void AssignImage()
    {
        ItemAdRewardInstance itemReward = rewardedAdsButtonManager.currentReward as ItemAdRewardInstance;
        itemImage.sprite = itemReward.itemRewardData.RewardIcon;
    }

    private void AssignProgressBarFill()
    {
        float limitTime = rewardedAdsButtonManager.currentReward.GetLimitTime();
        float remainingTime = rewardedAdsButtonManager.currentReward.GetRemainingTime();
        float alpha = 1f - (remainingTime / limitTime);

        progressBar.fillAmount = alpha;
    }

    private void OnButtonReleased()
    {
        rewardedAdsManager.SetCurrentReward(rewardedAdsButtonManager.currentReward);
        rewardedAdsMenu.Open();
    }
}