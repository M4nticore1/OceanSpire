using System;
using UnityEngine;

public class ReviveManager : MonoBehaviour
{
    private static ReviveManager instance;
    public static ReviveManager Instance => instance;

    [SerializeField] private RewardedAdsManager rewardedAdsManager;

    [SerializeField] private ReviveAdRewardDefinition reviveRewardDefinition;
    public ReviveAdRewardDefinition ReviveRewardDefinition => reviveRewardDefinition;

    [SerializeField] private int maxRevivesCount = 3;
    public int MaxRevivesCount => maxRevivesCount;

    [SerializeField] private int chargeReviveTimeInSeconds = 900;
    public int ChargeReviveTimeInSeconds => chargeReviveTimeInSeconds;

    public int RemainingRevivesCount { get; private set; } = 0;
    public long? NextChargeReviveTimeInSeconds { get; private set; } = null;

    public event Action<int> OnRevivesCountChanged;

    private void Awake()
    {
        if (instance) {
            Debug.Log("There's an extra ReviveCitizenManager on the scene!");
            Destroy(gameObject);

            return;
        }

        instance = this;
    }

    private void Update()
    {
        if (RemainingRevivesCount >= maxRevivesCount) return;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (NextChargeReviveTimeInSeconds == null) {
            Debug.LogError("NextChargeReviveTimeInSeconds is not valid to charge revives");
            NextChargeReviveTimeInSeconds = currentTime + chargeReviveTimeInSeconds;
        }

        if (currentTime < NextChargeReviveTimeInSeconds) return;

        AddReviveCount();
    }

    public void Init()
    {
        var reviveData = new ReviveSystemData()
        {
            RemainingRevivesCount = maxRevivesCount,
            NextReviveChargeTimes = Array.Empty<long>()
        };

        Init(reviveData);
    }

    public void Init(ReviveSystemData reviveData)
    {
        if (reviveData == null) {
            Debug.LogError("reviveData is not valid");
            Init();
            return;
        }

        RemainingRevivesCount = Mathf.Min(reviveData.RemainingRevivesCount, maxRevivesCount);

        if (RemainingRevivesCount >= maxRevivesCount) return;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nextReviveChargeTimes = reviveData.NextReviveChargeTimes;

        if (nextReviveChargeTimes == null) {
            Debug.LogError("nextReviveChargeTimes is not valid to recharge revives count");
            RemainingRevivesCount = maxRevivesCount;
            return;
        }

        foreach (var chargeTime in nextReviveChargeTimes) {
            if (RemainingRevivesCount >= maxRevivesCount) break;

            if (chargeTime > currentTime) break;

            AddReviveCount();
        }
    }

    public void CreateRewardAndApply(Citizen citizen)
    {
        var reward = reviveRewardDefinition.CreateReward() as ReviveAdRewardInstance;
        reward.SetHuman(citizen);

        rewardedAdsManager.SetReward(reward);
    }

    public void RemoveReviveCount()
    {
        SetRevivesCount(RemainingRevivesCount - 1);
    }

    private void AddReviveCount()
    {
        SetRevivesCount(RemainingRevivesCount + 1);
    }

    private void SetRevivesCount(int value)
    {
        if (value == RemainingRevivesCount) return;

        RemainingRevivesCount = value;
        UpdateNextChargeReviveTime();

        OnRevivesCountChanged?.Invoke(RemainingRevivesCount);
    }

    private void UpdateNextChargeReviveTime()
    {
        if (RemainingRevivesCount >= maxRevivesCount) {
            NextChargeReviveTimeInSeconds = null;
        }
        else {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (NextChargeReviveTimeInSeconds != null && NextChargeReviveTimeInSeconds.Value > currentTime && NextChargeReviveTimeInSeconds.Value - currentTime <= chargeReviveTimeInSeconds) return;

            NextChargeReviveTimeInSeconds = currentTime + chargeReviveTimeInSeconds;
        }
    }
}