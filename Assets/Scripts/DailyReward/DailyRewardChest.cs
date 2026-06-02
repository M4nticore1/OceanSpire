using System;
using UnityEngine;

public class DailyRewardChest : MonoBehaviour, IClickable
{
    [SerializeField] private DailyRewardMenu dailyRewardMenu;
    [SerializeField] private GameObject content;

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

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
        if (!DailyRewardManager.Instance.AdRewardCollected) return;

        content.SetActive(false);
    }

    private void OnDailyRewardsReset()
    {
        content.SetActive(true);
    }
}