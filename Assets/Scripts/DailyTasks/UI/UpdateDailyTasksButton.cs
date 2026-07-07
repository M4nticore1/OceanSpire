using UnityEngine;

public class UpdateDailyTasksButton : MonoBehaviour
{
    [SerializeField] private UpdateDailyTasksAdRewardDefinition rewardDefinition;
    [SerializeField] private CustomButton button;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnClicked);
        DailyTasksManager.Instance.OnAdUpdateUsedSetTrue += OnUpdateSetedTrue;
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnClicked);
        DailyTasksManager.Instance.OnAdUpdateUsedSetTrue -= OnUpdateSetedTrue;
    }

    private void OnClicked()
    {
        RewardedAdsManager.Instance.SetReward(rewardDefinition);
        RewardedAdsManager.Instance.ShowAd();
    }

    private void OnUpdateSetedTrue(bool value)
    {
        button.SetState(value ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }
}