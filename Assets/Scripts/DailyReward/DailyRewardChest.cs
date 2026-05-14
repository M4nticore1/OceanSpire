using UnityEngine;

public class DailyRewardChest : MonoBehaviour, IClickable
{
    [SerializeField] private DailyRewardMenu dailyRewardMenu;
    [SerializeField] private GameObject content;

    private void OnEnable()
    {
        DailyRewardManager.Instance.OnDailyRewardRecieved += OnDailyRewardRecieved;
        DailyRewardManager.Instance.OnDailyRewardReset += OnDailyRewardsReset;
    }

    private void OnDisable()
    {
        DailyRewardManager.Instance.OnDailyRewardRecieved -= OnDailyRewardRecieved;
        DailyRewardManager.Instance.OnDailyRewardReset -= OnDailyRewardsReset;
    }

    public void Click()
    {
        dailyRewardMenu.Open();
    }

    public bool ShouldClick()
    {
        return !DailyRewardManager.Instance.AdRewardCollected;
    }

    private void OnDailyRewardRecieved(RewardInstance reward)
    {
        if (!DailyRewardManager.Instance.AdRewardCollected) return;

        content.SetActive(false);
    }

    private void OnDailyRewardsReset()
    {
        content.SetActive(true);
    }
}