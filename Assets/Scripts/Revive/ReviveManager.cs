using System;
using System.Collections.Generic;
using UnityEngine;

public class ReviveManager : MonoBehaviour
{
    public static ReviveManager Instance { get; private set; }

    [SerializeField] private RewardedAdsManager rewardedAdsManager;

    [SerializeField] private ReviveAdRewardDefinition reviveRewardDefinition;
    public ReviveAdRewardDefinition ReviveRewardDefinition => reviveRewardDefinition;

    [SerializeField] private int maxRevivesCount = 3;
    public int MaxRevivesCount => maxRevivesCount;

    [SerializeField] private int chargeReviveTimeInSeconds = 900;
    public int ChargeReviveTimeInSeconds => chargeReviveTimeInSeconds;

    private HashSet<ReviveComponent> reviveComponents = new();

    public int RemainingRevivesCount { get; private set; } = 0;
    public long? NextChargeReviveTimeInSeconds { get; private set; } = null;

    public event Action<int> OnRevivesCountChanged;

    private void Awake()
    {
        if (Instance) {
            Debug.LogError($"[{nameof(ReviveManager)}] Another instance already exists in the scene! Destroying this.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        UpdateManager();
        UpdateComponents();
    }

    private void UpdateManager()
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

    private void UpdateComponents()
    {
        foreach (var component in reviveComponents) {
            component.Tick();
        }
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
            Debug.LogError($"[{nameof(ReviveManager)}] Next Revive Charge Times is not valid to recharge revives count");
            RemainingRevivesCount = maxRevivesCount;
            return;
        }

        foreach (var chargeTime in nextReviveChargeTimes) {
            if (RemainingRevivesCount >= maxRevivesCount) break;

            if (chargeTime > currentTime) break;

            AddReviveCount();
        }
    }

    public void RegisterReviveComponent(ReviveComponent component)
    {
        if (!component) return;

        reviveComponents.Add(component);
    }

    public void UnregisterReviveComponent(ReviveComponent component)
    {
        if (!component) return;

        reviveComponents.Remove(component);
    }

    public void CreateRewardAndApply(Citizen citizen)
    {
        var reward = reviveRewardDefinition.CreateReward() as ReviveAdRewardInstance;
        if (reward == null) {
            Debug.Log($"[{nameof(ReviveManager)}] Revive Reward is not valid!");
            return;
        }

        reward.SetHuman(citizen);

        rewardedAdsManager.SetReward(reward);
        rewardedAdsManager.ShowAd();
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