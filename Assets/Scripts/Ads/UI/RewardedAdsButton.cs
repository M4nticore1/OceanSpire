using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardedAdsButton : UIBehaviour
{
    [SerializeField] private RewardedAdsManager rewardedAdsManager;
    [SerializeField] private RewardedAdsButtonManager rewardedAdsButtonManager;
    [SerializeField] private RewardedAdsMenu rewardedAdsMenu;
    [SerializeField] private CustomButton button;
    [SerializeField] private Image itemImage;
    [SerializeField] private Image progressBar;

    private bool isShowed = false;

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
        isShowed = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        isShowed = false;
    }

    private void AssignImage()
    {
        itemImage.sprite = rewardedAdsButtonManager.currentReward.rewardData.RewardIcon;
    }

    private void AssignProgressBarFill()
    {
        float limitTime = rewardedAdsButtonManager.currentReward.limitTime;
        float remainingTime = rewardedAdsButtonManager.currentReward.remainingTime;
        float alpha = 1f - (remainingTime / limitTime);

        progressBar.fillAmount = alpha;
    }

    private void OnButtonReleased()
    {
        rewardedAdsManager.SetCurrentReward(rewardedAdsButtonManager.currentReward);
        rewardedAdsMenu.Open();
    }
}
