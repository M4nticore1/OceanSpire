using UnityEngine;

public class DailyTasksRewardManager : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;

    private void OnEnable()
    {
        DailyTaskInstance.OnTaskCompleted += HandleDailyTaskCompleted;
    }

    private void OnDisable()
    {
        DailyTaskInstance.OnTaskCompleted -= HandleDailyTaskCompleted;
    }

    private void HandleDailyTaskCompleted(DailyTaskInstance dailyTaskInstance)
    {
        if (dailyTaskInstance == null) return;

        cityStorage.Inventory.AddItemAmount(dailyTaskInstance.Reward);
    }
}