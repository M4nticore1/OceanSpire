using UnityEngine;

public class RewardedAdsButtonManager : MonoBehaviour
{
    [SerializeField] private AppLovinMaxAdsManager appLovinMaxAds;
    [SerializeField] private RewardedAdsButton rewardedAdsButton;
    [SerializeField] private RewardedAdsMenu rewardedAdsMenu;

    [SerializeField] private float adShowTime = 30f;
    [SerializeField] private float adCooldownTime = 2f;

    public AdRewardInstance currentReward { get; private set; }
    private bool isAdButtonShowed = false;

    private double currentToggleTime = 0d;

    private void OnEnable()
    {
        EventBus.onAdRewardRecieved += OnAdRewardReceived;
    }

    private void OnDisable()
    {
        EventBus.onAdRewardRecieved -= OnAdRewardReceived;
    }

    private void Start()
    {
        HideAdButton();
    }

    private void Update()
    {
        currentToggleTime += Time.deltaTime;

        if (isAdButtonShowed) {
            ProcessReduceRewardRemainingTime();

            if (currentToggleTime >= adShowTime) {
                currentReward = null;

                HideAdButton();
                currentToggleTime = 0f;
            }
        }
        else {
            if (currentToggleTime >= adCooldownTime) {
                currentReward = GetRandomAdReward();
                currentReward.SetLimitTime(adShowTime);

                ShowAdButton();
                currentToggleTime = 0f;
            }
        }
    }

    private void ShowAdButton()
    {
        rewardedAdsButton.Show();
        isAdButtonShowed = true;
    }

    private void HideAdButton()
    {
        rewardedAdsButton.Hide();
        isAdButtonShowed = false;
    }

    private void ProcessReduceRewardRemainingTime()
    {
        currentReward.ReduceRemainingTime(Time.deltaTime);
    }

    private void OnAdRewardReceived(AdRewardInstance reward)
    {
        if (reward != currentReward) return;

        HideAdButton();
        currentToggleTime = 0f;
        currentReward = null;
    }

    private AdRewardInstance GetRandomAdReward()
    {
        int length = AdRewardsList.Instance.AdRewards.Length;
        int index = Random.Range(0, length);

        AdRewardData data = AdRewardsList.Instance.AdRewards[index];

        return data.CreateInstance();
    }
}