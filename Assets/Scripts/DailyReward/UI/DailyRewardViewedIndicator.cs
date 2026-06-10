using UnityEngine;

public class DailyRewardViewedIndicator : MonoBehaviour
{
    [SerializeField] private DailyRewardManager dailyRewardManager;
    [SerializeField] private GameObject indicator;

    private void OnEnable()
    {
        dailyRewardManager.OnRewardViewedChanged += OnDailyRewardViewedChanged;
    }

    private void OnDisable()
    {
        dailyRewardManager.OnRewardViewedChanged -= OnDailyRewardViewedChanged;
    }

    private void OnDailyRewardViewedChanged(bool value)
    {
        if (value) {
            HideIndicator();
        }
        else {
            ShowIndicator();
        }
    }

    private void ShowIndicator()
    {
        indicator.SetActive(true);
    }

    private void HideIndicator()
    {
        indicator.SetActive(false);
    }
}