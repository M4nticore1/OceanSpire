using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour, ILocalizable
{
    public static DailyRewardManager Instance;

    [SerializeField] private ItemAdRewardDefinition[] rewards;

    [SerializeField] private int maxRewardsCount = 4;
    public int MaxRewardsCount => maxRewardsCount;

    [SerializeField] private int updateRewardTimeOffset = 0;

    [Header("Receive Reward")]
    [SerializeField] private int maxFreeRewardsCount = 1;
    [SerializeField] private int maxAdRewardsCount = 1;

    private int currentFreeRecievesCount = 0;
    private int currentAdRecievesCount = 0;

    private long nextResetSeconds = 0;

    private List<ItemAdRewardInstance> currentRewards = new();

    public event Action onDailyRewardReset;
    public event Action<AdRewardInstance> onDailyRewardRecieved;

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
        AdRewardInstance.onRewardReceived += OnRewardRecieved;
    }

    private void OnDisable()
    {
        AdRewardInstance.onRewardReceived -= OnRewardRecieved;
    }

    private void Update()
    {
        long currentSecond = TimeManager.GetCurrentSecond();
        if (currentSecond < nextResetSeconds) return;

        ResetRewards();
    }

    public void Init(DailyRewardData data)
    {
        if (data != null) {
            nextResetSeconds = data.NextUpdateSeconds;

            foreach (ItemData item in data.Items) {
                var id = item.Id;
                var reward = rewards[id].CreateInstance() as ItemAdRewardInstance;

                reward.SetAmount(item.Amount);
                currentRewards.Add(reward);
            }
        }
        else {
            ResetRewards();
        }
    }

    public bool CanSelectFreeReward()
    {
        return currentFreeRecievesCount < maxFreeRewardsCount;
    }

    public bool CanSelectReward()
    {
        return currentAdRecievesCount < maxAdRewardsCount;
    }

    public ItemAdRewardInstance GetCurrentReward(int id)
    {
        return currentRewards[id];
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "remainingHours", GetRemainingResetHours() },
            { "remainingMinutes", GetRemainingResetMinutes()}
        };
    }

    private void UpdateNextResetSeconds()
    {
        long minTargetSecond = updateRewardTimeOffset * 3600;
        long maxTargetSecond = (24 + updateRewardTimeOffset) * 3600;
        long targetSecond = minTargetSecond - TimeManager.GetCurrentSecond() >= 0 ? minTargetSecond : maxTargetSecond;

        nextResetSeconds = TimeManager.GetCurrentSecond() + 24 * 3600;
    }

    private void ResetRewards()
    {
        UpdateRewards();
        UpdateNextResetSeconds();
        onDailyRewardReset?.Invoke();
    }

    private void UpdateRewards()
    {
        List<int> rewardIds = new();

        for (int i = 0; i < maxRewardsCount; i++) {
            var id = UnityEngine.Random.Range(0, rewards.Length);

            while (rewardIds.Contains(id) && rewardIds.Count < rewards.Length) {
                id = UnityEngine.Random.Range(0, rewards.Length);
            }

            rewardIds.Add(id);

            var reward = rewards[id].CreateInstance() as ItemAdRewardInstance;
            reward.SetAmountPercent(GameStageSystem.CalculateGameStagePercent());

            currentRewards.Add(reward);
        }
    }

    private void OnRewardRecieved(AdRewardInstance reward)
    {
        if (!rewards.Contains(reward.Definition)) return;

        if (CanSelectFreeReward()) {
            currentFreeRecievesCount++;
        }
        else {
            currentAdRecievesCount++;
        }

        onDailyRewardRecieved?.Invoke(reward);
    }

    private string GetRemainingResetHours()
    {
        long currentSeconds = TimeManager.GetCurrentSecond();
        long remainingTime = nextResetSeconds - currentSeconds;

        int hours = (int)((float)remainingTime / 3600);
        string text = hours.ToString();

        return text;
    }

    private string GetRemainingResetMinutes()
    {
        long currentSeconds = TimeManager.GetCurrentSecond();
        long remainingTime = nextResetSeconds - currentSeconds;

        float hours = (float)remainingTime / 3600;
        int minutes = (int)((hours - (int)hours) * 60);
        string text = minutes.ToString();

        return text;
    }
}