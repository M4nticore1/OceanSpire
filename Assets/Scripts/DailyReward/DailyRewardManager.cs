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

    private List<RewardInstance> currentRewards = new();
    public IReadOnlyList<RewardInstance> CurrentRewards => currentRewards;

    public event Action OnDailyRewardReset;
    public event Action<RewardInstance> OnDailyRewardRecieved;

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
    }

    public void Init(DailyRewardData data)
    {
        foreach (var rewardData in data.Rewards) {
            var definition = rewardsList.GetRewardDefinition(rewardData.Id);
            var reward = definition.CreateReward();

            if (reward is ItemRewardInstance itemReward) {
                itemReward.SetAmountPercent(GameStageSystem.CalculateGameStagePercent());
            }

            reward.SetCollected(rewardData.Collected);

            currentRewards.Add(reward);
        }

        NextResetTime = data.NextResetTime;
        FreeRewardCollected = data.FreeRewardCollected;
        AdRewardCollected = data.AdRewardCollected;
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
            int randomIndex = UnityEngine.Random.Range(0, availableRewards.Count);

            var def = availableRewards[randomIndex];
            availableRewards.RemoveAt(randomIndex);

            var reward = def.CreateReward();
            if (reward is ItemRewardInstance itemReward) {
                itemReward.SetAmountPercent(GameStageSystem.CalculateGameStagePercent());
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

    private void UpdateRewards()
    {
        foreach (var rewardData in GetRandomRewardsData()) {
            currentRewards.Add(rewardData.CreateReward());
        }
    }

    private void ResetRewards()
    {
        UpdateRewards();
        UpdateNextResetTime();
        UpdateRewardCollected();
        OnDailyRewardReset?.Invoke();
    }

    private void UpdateNextResetTime()
    {
        NextResetTime = CalculateNextResetTime();
    }

    private void UpdateRewardCollected()
    {
        FreeRewardCollected = false;
        AdRewardCollected = false;
    }

    private void OnRewardRecieved(RewardInstance reward)
    {
        if (!currentRewards.Contains(reward)) return;

        if (!FreeRewardCollected) {
            FreeRewardCollected = false;
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
}