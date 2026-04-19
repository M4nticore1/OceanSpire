using System;
using UnityEngine;

public class ReviveManager : MonoBehaviour
{
    private static ReviveManager instance;
    public static ReviveManager Instance => instance;

    [SerializeField] private ReviveAdRewardDefinition reviveRewardDefinition;
    public ReviveAdRewardDefinition ReviveRewardDefinition => reviveRewardDefinition;

    [SerializeField] private int maxRevivesCount = 3;
    public int MaxRevivesCount => maxRevivesCount;

    [SerializeField] private float restoreReviveTime = 300f;
    private float currentRestoreReviveTime = 0f;

    public int RemainingRevivesCount { get; private set; } = 0;

    public static event Action onRewardCreated;
    public static event Action onRevivesCountChanged;

    private void OnEnable()
    {
        SelectManager.onComponentSelected += OnComponentSelected;
        Human.onHumanRevived += OnHumanRevived;
    }

    private void OnDisable()
    {
        SelectManager.onComponentSelected -= OnComponentSelected;
        Human.onHumanRevived -= OnHumanRevived;
    }

    private void Awake()
    {
        if (instance) {
            Debug.Log("There's an extra ReviveCitizenManager on the scene!");
            Destroy(gameObject);

            return;
        }

        instance = this;
    }

    private void Start()
    {
        SetRevivesCount(maxRevivesCount);
    }

    private void Update()
    {
        if (RemainingRevivesCount >= maxRevivesCount) return;

        currentRestoreReviveTime += Time.deltaTime;
        if (currentRestoreReviveTime < restoreReviveTime) return;

        AddReviveCount();
        ResetRestoreReviveTime();
    }

    private void SetRevivesCount(int value)
    {
        if (RemainingRevivesCount == value) return;

        RemainingRevivesCount = value;
        onRevivesCountChanged?.Invoke();
    }

    private void AddReviveCount()
    {
        SetRevivesCount(RemainingRevivesCount++);
    }

    private void RemoveReviveCount()
    {
        SetRevivesCount(RemainingRevivesCount--);
    }

    private void ResetRestoreReviveTime()
    {
        currentRestoreReviveTime = 0f;
    }

    private void CreateReward(Human human)
    {
        float time = human.ReviveHandler.ReviveLimitTime - human.ReviveHandler.CurrentDiedTime;
        ReviveAdRewardInstance reward = reviveRewardDefinition.CreateInstance(time) as ReviveAdRewardInstance;
        reward.SetHuman(human);

        RewardedAdsManager.instance.SetCurrentReward(reward);
        onRewardCreated?.Invoke();
    }

    private void OnComponentSelected(SelectComponent component)
    {
        Human human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return;

        if (human.Health.isAlive) return;

        CreateReward(human);
    }

    private void OnHumanRevived(Human human)
    {
        RemoveReviveCount();
    }
}