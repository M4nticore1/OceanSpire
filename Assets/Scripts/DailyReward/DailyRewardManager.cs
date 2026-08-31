using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour, ILocalizable
{
    public static DailyRewardManager Instance;

    [SerializeField] private RewardsList rewardsList;
    [SerializeField] private AdRewardDefinition[] rewards;

    [SerializeField] private int maxRewardsCount = 4;
    public int MaxRewardsCount => maxRewardsCount;

    [SerializeField] private int updateRewardTimeOffset = 3;

    public bool MainRewardCollected { get; private set; } = false;
    public bool ExtraRewardCollected { get; private set; } = false;
    public long NextResetTime { get; private set; } = 0;
    public bool IsRewardViewed { get; private set; } = false;

    private List<RewardInstance> currentRewards = new();
    public IReadOnlyList<RewardInstance> CurrentRewards => currentRewards;

    public event Action OnDailyRewardReset;
    public event Action<RewardInstance> OnDailyRewardRecieved;
    public event Action<bool> OnRewardViewedChanged;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        RewardInstance.OnRewardReceived += OnRewardRecieved;
    }

    private void OnDisable()
    {
        RewardInstance.OnRewardReceived -= OnRewardRecieved;
    }

    private void Update()
    {
        long currentSecond = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentSecond < NextResetTime) return;

        ResetRewardCollected();
        SetRewardViewed(false);
        UpdateNextResetTime();
        UpdateRewards();

        OnDailyRewardReset?.Invoke();
    }

    public void Init()
    {
        var newData = new DailyRewardData
        {
            Rewards = GetRandomRewardsData(),
            NextResetTime = CalculateNextResetTime(),
            MainRewardCollected = false,
            ExtraRewardCollected = false,
            RewardViewed = false,
        };

        Init(newData);
    }

    public void Init(DailyRewardData dailyRewardData)
    {
        if (dailyRewardData == null) {
            Debug.LogError($"[{nameof(DailyRewardManager)}] Daily Reward Data is not valid");
            Init();
            return;
        }

        foreach (var rewardData in dailyRewardData.Rewards) {
            if (rewardData == null) continue;

            int id = rewardData.Id;
            var reward = TryCreateReward(id);
            if (reward == null) continue;

            reward.SetCollected(rewardData.Collected);
            currentRewards.Add(reward);
        }

        if (currentRewards.Count < maxRewardsCount) {
            var existingIds = new HashSet<RewardId>();
            foreach (var r in currentRewards) {
                existingIds.Add(r.Definition.RewardId);
            }

            var availablePool = new List<AdRewardDefinition>(rewards);

            while (currentRewards.Count < maxRewardsCount && availablePool.Count > 0) {
                int index = UnityEngine.Random.Range(0, availablePool.Count);
                var definition = availablePool[index];
                availablePool.RemoveAt(index);

                if (definition == null) continue;
                if (existingIds.Contains(definition.RewardId)) {
                    continue;
                }

                var reward = TryCreateReward((int)definition.RewardId);
                if (reward != null) {
                    currentRewards.Add(reward);
                    existingIds.Add(definition.RewardId);
                }
            }
        }

        while (currentRewards.Count > maxRewardsCount) {
            currentRewards.RemoveAt(currentRewards.Count - 1);
        }

        NextResetTime = dailyRewardData.NextResetTime;
        MainRewardCollected = dailyRewardData.MainRewardCollected;
        ExtraRewardCollected = dailyRewardData.ExtraRewardCollected;
        SetRewardViewed(dailyRewardData.RewardViewed);
    }

    public void SetRewardViewed(bool value)
    {
        IsRewardViewed = value;
        OnRewardViewedChanged?.Invoke(value);
    }

    public RewardInstance GetCurrentReward(int id)
    {
        return currentRewards[id];
    }

    public RewardInstanceData[] GetRandomRewardsData()
    {
        var rewardsData = new List<RewardInstanceData>();
        var availableRewards = new List<AdRewardDefinition>(rewards);

        int count = Mathf.Min(maxRewardsCount, availableRewards.Count);

        for (int i = 0; i < count; i++) {
            if (availableRewards.Count == 0) break;

            var index = UnityEngine.Random.Range(0, availableRewards.Count);
            var definition = availableRewards[index];
            availableRewards.RemoveAt(index);

            if (definition == null) {
                Debug.LogWarning($"Reward Definition is null at index {index}");
                continue;
            }

            var id = (int)definition.RewardId;
            var reward = TryCreateReward(id);

            if (reward == null) {
                Debug.LogWarning($"[{nameof(DailyRewardManager)}] Reward with ID {id} could not be created");
                continue;
            }

            var rewardData = reward.CreateData();
            if (rewardData != null) {
                rewardsData.Add(rewardData);
            }
        }

        return rewardsData.ToArray();
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "remainingTime", TimeFormatter.SecondsToHourTimer(GetRemainingTime()) }
        };
    }

    public long CalculateNextResetTime()
    {
        var now = DateTime.UtcNow;
        var nextReset = new DateTime(now.Year, now.Month, now.Day, updateRewardTimeOffset, 0, 0, DateTimeKind.Utc);

        if (nextReset <= now) {
            nextReset = nextReset.AddDays(1);
        }

        return ((DateTimeOffset)nextReset).ToUnixTimeSeconds();
    }

    private void UpdateRewards()
    {
        currentRewards.Clear();

        foreach (var rewardData in GetRandomRewardsData()) {
            if (rewardData == null) continue;

            var reward = rewardData.CreateReward();
            if (reward == null) {
                Debug.LogError($"[{nameof(DailyRewardManager)}] Reward is not valid");
                continue;
            }

            currentRewards.Add(reward);
        }
    }

    private void UpdateNextResetTime()
    {
        NextResetTime = CalculateNextResetTime();
    }

    private void ResetRewardCollected()
    {
        MainRewardCollected = false;
        ExtraRewardCollected = false;
    }

    private void OnRewardRecieved(RewardInstance reward)
    {
        if (!currentRewards.Contains(reward)) return;

        if (!MainRewardCollected) {
            MainRewardCollected = true;
        }
        else {
            ExtraRewardCollected = true;
        }

        OnDailyRewardRecieved?.Invoke(reward);
    }

    private int GetRemainingTime()
    {
        long currentSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return (int)(NextResetTime - currentSeconds);
    }

    private RewardInstance TryCreateRandomReward()
    {
        int count = rewards.Length;
        int index = UnityEngine.Random.Range(0, count);
        int id = (int)rewards[index].RewardId;

        return TryCreateReward(id);
    }

    private RewardInstance TryCreateReward(int id)
    {
        var definition = rewardsList.GetRewardDefinition(id);
        if (definition == null) {
            Debug.Log($"[{nameof(DailyRewardManager)}] Reward Definition not found at {name}");
            return null;
        }

        var reward = definition.CreateReward();
        if (reward == null) {
            Debug.Log($"[{nameof(DailyRewardManager)}] Reward not found at {name}");
            return null;
        }

        if (reward is ItemRewardInstance itemReward) {
            itemReward.SetAmountPercent(GameStageSystem.CalculateGameStagePercent());
        }

        return reward;
    }
}