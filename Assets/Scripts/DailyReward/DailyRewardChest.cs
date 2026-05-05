using UnityEngine;

public class DailyRewardChest : MonoBehaviour, IClickable
{
    [SerializeField] private DailyRewardMenu dailyRewardMenu;
    [SerializeField] private GameObject content;

    private void OnEnable()
    {
        DailyRewardManager.Instance.onDailyRewardRecieved += OnDailyRewardRecieved;
        DailyRewardManager.Instance.onDailyRewardReset += OnDailyRewardsReset;
    }

    private void OnDisable()
    {
        DailyRewardManager.Instance.onDailyRewardRecieved -= OnDailyRewardRecieved;
        DailyRewardManager.Instance.onDailyRewardReset -= OnDailyRewardsReset;
    }

    public void Click()
    {
        dailyRewardMenu.Open();
    }

    public bool ShouldClick()
    {
        return DailyRewardManager.Instance.CanSelectReward();
    }

    private void OnDailyRewardRecieved(AdRewardInstance reward)
    {
        if (DailyRewardManager.Instance.CanSelectReward()) return;

        content.SetActive(false);
    }

    private void OnDailyRewardsReset()
    {
        content.SetActive(true);
    }
}