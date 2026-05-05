using UnityEngine;

public class UpdateDailyTasksButton : MonoBehaviour
{
    [SerializeField] private UpdateDailyTasksAdRewardDefinition rewardDefinition;
    [SerializeField] private CustomButton button;

    private void OnEnable()
    {
        button.onReleased.AddListener(OnClicked);
        DailyTasksManager.Instance.onAdUpdateUsedSetTrue += OnUpdateSetedTrue;
    }

    private void OnDisable()
    {
        button.onReleased.RemoveListener(OnClicked);
        DailyTasksManager.Instance.onAdUpdateUsedSetTrue -= OnUpdateSetedTrue;
    }

    private void OnClicked()
    {
        RewardedAdsManager.Instance.SetCurrentReward(rewardDefinition);
        RewardedAdsManager.Instance.ShowAd();
    }

    private void OnUpdateSetedTrue(bool value)
    {
        button.SetState(value ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }
}