using UnityEngine;

public class DailyTasksIndicator : MonoBehaviour
{
    [SerializeField] private DailyTasksManager dailyTasksManager;
    [SerializeField] private GameObject indicator;

    private void OnEnable()
    {
        dailyTasksManager.OnTasksViewedChanged += OnDailyTasksViewedChanged;
    }

    private void OnDisable()
    {
        dailyTasksManager.OnTasksViewedChanged -= OnDailyTasksViewedChanged;
    }

    private void OnDailyTasksViewedChanged(bool value)
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