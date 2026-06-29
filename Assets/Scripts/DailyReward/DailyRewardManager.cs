using System;
using System.Collections.Generic;
using Unity.Mathematics;
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
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        RewardInstance.onRewardReceived += OnRewardRecieved;
    }

    private void OnDisable()
    {
        RewardInstance.onRewardReceived -= OnRewardRecieved;
    }

    private void Update()
    {
        long currentSecond = TimeManager.GetCurrentSecond();
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
            Debug.LogError("dailyRewardData is not valid");
            return;
        }

        foreach (var rewardData in dailyRewardData.Rewards) {
            if (rewardData == null) {
                Debug.LogError($"Reward Data not found at {name}");
                continue;
            }

            int id = rewardData.Id;

            var reward = TryCreateReward(id);
            if (reward == null) {
                Debug.LogError($"Reward not found at {name}");
            }

            reward.SetCollected(rewardData.Collected);
            currentRewards.Add(reward);
        }

        while (currentRewards.Count > maxRewardsCount) {
            currentRewards.RemoveAt(currentRewards.Count - 1);
        }

        while (currentRewards.Count < maxRewardsCount) {
            var reward = TryCreateRandomReward();
            if (reward == null) {
                Debug.LogError($"Reward not found at {name}");
            }

            currentRewards.Add(reward);
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

            int index = UnityEngine.Random.Range(0, availableRewards.Count);
            var definition = availableRewards[index];
            availableRewards.RemoveAt(index);

            if (!definition) {
                Debug.LogWarning($"Reward Definition is null at index {index}");
                continue;
            }

            int id = (int)definition.RewardId;
            var reward = TryCreateReward(id);

            if (reward == null) {
                Debug.LogWarning($"Reward with ID {id} could not be created");
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
            { "remainingHours", GetRemainingResetHours() },
            { "remainingMinutes", GetRemainingResetMinutes()}
        };
    }

    public long CalculateNextResetTime()
    {
        DateTime now = DateTime.UtcNow;
        DateTime nextReset = new DateTime(now.Year, now.Month, now.Day, updateRewardTimeOffset, 0, 0, DateTimeKind.Utc);

        if (nextReset <= now) {
            nextReset = nextReset.AddDays(1);
        }

        return ((DateTimeOffset)nextReset).ToUnixTimeSeconds();
    }

    private void UpdateRewards()
    {
        currentRewards.Clear();

        foreach (var rewardData in GetRandomRewardsData()) {
            var reward = rewardData.CreateReward();
            if (reward == null) {
                Debug.LogError("reward is not valid");
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

    private string GetRemainingResetHours()
    {
        long currentSeconds = TimeManager.GetCurrentSecond();
        long remainingTime = NextResetTime - currentSeconds;

        int hours = (int)((float)remainingTime / 3600);
        string text = hours.ToString();

        return text;
    }

    private string GetRemainingResetMinutes()
    {
        long currentSeconds = TimeManager.GetCurrentSecond();
        long remainingTime = NextResetTime - currentSeconds;

        float hours = (float)remainingTime / 3600;
        int minutes = (int)((hours - (int)hours) * 60);
        string text = minutes.ToString();

        return text;
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
        if (!definition) {
            Debug.Log($"Reward Definition not found at {name}");
            return null;
        }

        var reward = definition.CreateReward();
        if (reward == null) {
            Debug.Log($"Reward not found at {name}");
            return null;
        }

        if (reward is ItemRewardInstance itemReward) {
            itemReward.SetAmountPercent(GameStageSystem.CalculateGameStagePercent());
        }

        return reward;
    }
}