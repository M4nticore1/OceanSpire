using UnityEngine;

public class UpdateDailyTasksButton : MonoBehaviour
{
    [SerializeField] private RewardedAdsManager rewardedAdsManager;
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
        rewardedAdsManager.SetReward(new UpdateDailyTasksAdRewardInstance());
        rewardedAdsManager.ShowAd();
    }

    private void OnUpdateSetedTrue(bool value)
    {
        button.SetState(value ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }
}