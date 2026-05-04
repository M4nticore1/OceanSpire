using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BonusChestManager : MonoBehaviour
{
    public static BonusChestManager Instance;

    [SerializeField] private ItemAdRewardDefinition[] rewards;

    [SerializeField] private int maxRewardsCount = 4;
    public int MaxRewardsCount => maxRewardsCount;

    [SerializeField] private int updateFrequencyInHours = 24;

    [Header("Receive Reward")]
    [SerializeField] private int maxFreeRewardsCount = 1;
    [SerializeField] private int maxAdRewardsCount = 1;

    private int currentFreeRecievesCount = 0;
    private int currentAdRecievesCount = 0;

    private long nextUpdateSeconds = 0;

    private List<ItemAdRewardInstance> currentRewards = new();

    public event Action onBonusChestUpdated;
    public event Action<AdRewardInstance> onRewardRecieved;

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
        if (currentSecond < nextUpdateSeconds) return;

        UpdateRewards();
        UpdateNextUpdateSeconds();
        onBonusChestUpdated?.Invoke();
    }

    public void Init(BonusChestData data)
    {
        if (data != null) {
            nextUpdateSeconds = data.NextUpdateSeconds;

            foreach (ItemData item in data.Items) {
                var id = item.Id;
                var reward = rewards[id].CreateInstance() as ItemAdRewardInstance;

                reward.SetAmount(item.Amount);
                currentRewards.Add(reward);
            }
        }
        else {
            UpdateRewards();
        }
    }

    public bool CanTakeFreeReward()
    {
        return currentFreeRecievesCount < maxFreeRewardsCount;
    }

    public bool CanTakeReward()
    {
        return currentAdRecievesCount < maxAdRewardsCount;
    }

    public ItemAdRewardInstance GetCurrentReward(int id)
    {
        return currentRewards[id];
    }

    private void UpdateNextUpdateSeconds()
    {
        nextUpdateSeconds = TimeManager.GetCurrentSecond() + updateFrequencyInHours * 60 * 60;
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

        if (CanTakeFreeReward()) {
            currentFreeRecievesCount++;
        }
        else {
            currentAdRecievesCount++;
        }

        onRewardRecieved?.Invoke(reward);
    }
}