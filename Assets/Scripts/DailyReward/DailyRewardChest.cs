using System;
using UnityEngine;

public class DailyRewardChest : MonoBehaviour, IClickable
{
    [SerializeField] private DailyRewardManager dailyRewardManager;
    [SerializeField] private DailyRewardMenu dailyRewardMenu;
    [SerializeField] private GameObject content;

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

    [SerializeField] private Movement movement;

    private void OnEnable()
    {
        dailyRewardManager.OnDailyRewardRecieved += OnDailyRewardRecieved;
        dailyRewardManager.OnDailyRewardReset += OnDailyRewardsReset;
    }

    private void OnDisable()
    {
        dailyRewardManager.OnDailyRewardRecieved -= OnDailyRewardRecieved;
        dailyRewardManager.OnDailyRewardReset -= OnDailyRewardsReset;
    }

    public void Click()
    {
        dailyRewardMenu.Show();

        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        IsClickable = value;
    }

    public bool ShouldClick()
    {
        if (!IsClickable) return false;

        return true;
    }

    private void OnDailyRewardRecieved(RewardInstance reward)
    {
        if (!dailyRewardManager.ExtraRewardCollected) return;

        content.SetActive(false);
    }

    private void OnDailyRewardsReset()
    {
        content.SetActive(true);
    }
}