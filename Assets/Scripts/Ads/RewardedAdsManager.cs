using UnityEngine;

public class RewardedAdsManager : MonoBehaviour
{
    [SerializeField] private RewardedAdsButton rewardedAdButton;
    [SerializeField] private AppLovinMaxAdsManager appLovinMaxAds;

    public AdReward currentReward { get; private set; }

    private bool isAdButtonShowed = false;

    private double currentToggleTime = 0d;
    private double lastToggleTime = 0d;
    private const float AdButtonShowTime = 10f;
    private const float AdButtonHideTime = 10f;

    private void Start()
    {
        HideAdButton();
    }

    private void Update()
    {
        currentToggleTime += Time.deltaTime;

        if (isAdButtonShowed) {
            if (currentToggleTime >= lastToggleTime + AdButtonShowTime) {
                HideAdButton();
                lastToggleTime = currentToggleTime;
            }
        }
        else {
            if (currentToggleTime >= lastToggleTime + AdButtonHideTime) {
                SetReward(GetRandomAdReward());
                ShowAdButton();
                lastToggleTime = currentToggleTime;
            }
        }
    }

    public void HandleRewardedAdReceivedReward()
    {
        if (currentReward == null) return;

        currentReward.RecieveReward();
        HideAdButton();
        SetReward(null);
    }

    private void SetReward(AdReward reward)
    {
        currentReward = reward;
    }

    private void ShowAdButton()
    {
        rewardedAdButton.gameObject.SetActive(true);
        isAdButtonShowed = true;
    }

    private void HideAdButton()
    {
        rewardedAdButton.gameObject.SetActive(false);
        isAdButtonShowed = false;
    }

    private AdReward GetRandomAdReward()
    {
        int length = AdRewardsList.Instance.AdRewards.Length;
        int index = Random.Range(0, length - 1);

        return AdRewardsList.Instance.AdRewards[index];
    }
}