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

    public bool FreeRewardCollected { get; private set; } = false;
    public bool AdRewardCollected { get; private set; } = false;
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

        ResetRewards();
        ResetNextResetTime();
        ResetRewardCollected();
        SetRewardViewed(false);
        OnDailyRewardReset?.Invoke();
    }

    public void Init()
    {
        var newData = new DailyRewardData
        {
            Rewards = GetRandomRewardsData(),
            NextResetTime = CalculateNextResetTime(),
            FreeRewardCollected = false,
            AdRewardCollected = false,
            RewardViewed = false,
        };

        Init(newData);
    }

    public void Init(DailyRewardData data)
    {
        foreach (var rewardData in data.Rewards) {
            if (rewardData == null) {
                Debug.Log($"Reward Data not found at {name}");
                continue;
            }

            int id = rewardData.Id;

            var reward = TryCreateReward(id);
            if (reward == null) {
                Debug.Log($"Reward not found at {name}");
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
                Debug.Log($"Reward not found at {name}");
            }

            currentRewards.Add(reward);
        }

        NextResetTime = data.NextResetTime;
        FreeRewardCollected = data.FreeRewardCollected;
        AdRewardCollected = data.AdRewardCollected;
        SetRewardViewed(data.RewardViewed);
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
        List<RewardInstanceData> rewardsData = new();
        List<AdRewardDefinition> availableRewards = new(rewards);

        int count = Mathf.Min(maxRewardsCount, availableRewards.Count);

        for (int i = 0; i < count; i++) {
            int index = UnityEngine.Random.Range(0, availableRewards.Count);

            var definition = availableRewards[index];
            if (!definition) {
                Debug.Log($"Reward Definition not found at {name} at index {index}");
                continue;
            }

            int id = (int)definition.RewardId;

            var reward = TryCreateReward(id);
            if (reward == null) {
                Debug.Log($"Reward not found at {name}");
            }

            var rewardData = reward.CreateData();
            rewardsData.Add(rewardData);
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
        long minTargetSecond = updateRewardTimeOffset * 3600;
        long maxTargetSecond = (24 + updateRewardTimeOffset) * 3600;
        long targetSecond = minTargetSecond - TimeManager.GetCurrentSecond() >= 0 ? minTargetSecond : maxTargetSecond;

        return targetSecond;
    }

    private void ResetRewards()
    {
        foreach (var rewardData in GetRandomRewardsData()) {
            currentRewards.Add(rewardData.CreateReward());
        }
    }

    private void ResetNextResetTime()
    {
        NextResetTime = CalculateNextResetTime();
    }

    private void ResetRewardCollected()
    {
        FreeRewardCollected = false;
        AdRewardCollected = false;
    }

    private void OnRewardRecieved(RewardInstance reward)
    {
        if (!currentRewards.Contains(reward)) return;

        if (!FreeRewardCollected) {
            FreeRewardCollected = true;
        }
        else {
            AdRewardCollected = true;
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