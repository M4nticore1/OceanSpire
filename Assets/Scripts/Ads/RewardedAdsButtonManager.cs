using UnityEngine;

public class RewardedAdsButtonManager : MonoBehaviour
{
    [SerializeField] private AppLovinMaxRewardedAdsSystem appLovinMaxAds;
    [SerializeField] private RewardedAdsButton rewardedAdsButton;

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
            if (currentToggleTime >= adShowTime) {
                currentReward = null;

                HideAdButton();
                currentToggleTime = 0f;
            }
        }
        else {
            if (currentToggleTime >= adCooldownTime) {
                currentReward = GetRandomAdReward().CreateInstance();

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

    private void OnAdRewardReceived(AdRewardInstance reward)
    {
        if (reward != currentReward) return;

        HideAdButton();
        currentToggleTime = 0f;
        currentReward = null;
    }

    private AdRewardDefinition GetRandomAdReward()
    {
        int length = AdRewardsList.Instance.AdRewards.Length;
        int index = Random.Range(0, length);

        AdRewardDefinition def = AdRewardsList.Instance.AdRewards[index];

        return def;
    }
}