using UnityEngine;

public class RewardedAdsButtonManager : MonoBehaviour
{
    [SerializeField] private AdRewardDefinition[] rewardDefinitions;
    //[SerializeField] private AppLovinMaxRewardedAdsSystem appLovinMaxAds;
    [SerializeField] private RewardedAdsButton rewardedAdsButton;

    [SerializeField] private float adShowTime = 30f;
    [SerializeField] private float adCooldownTime = 2f;

    public RewardInstance currentReward { get; private set; }
    private bool isAdButtonShowed = false;

    private double currentToggleTime = 0d;

    private void OnEnable()
    {
        RewardInstance.OnRewardReceived += OnAdRewardReceived;
    }

    private void OnDisable()
    {
        RewardInstance.OnRewardReceived -= OnAdRewardReceived;
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
                currentReward = GetRandomAdReward().CreateReward();

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

    private void OnAdRewardReceived(RewardInstance reward)
    {
        if (reward != currentReward) return;

        HideAdButton();
        currentToggleTime = 0f;
        currentReward = null;
    }

    private AdRewardDefinition GetRandomAdReward()
    {
        int length = rewardDefinitions.Length;
        int index = Random.Range(0, length);

        AdRewardDefinition def = rewardDefinitions[index];

        return def;
    }
}