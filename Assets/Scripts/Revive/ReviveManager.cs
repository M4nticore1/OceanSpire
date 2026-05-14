using System;
using UnityEngine;

public class ReviveManager : MonoBehaviour
{
    private static ReviveManager instance;
    public static ReviveManager Instance => instance;

    [SerializeField] private ReviveAdRewardDefinition reviveRewardDefinition;
    public ReviveAdRewardDefinition ReviveRewardDefinition => reviveRewardDefinition;

    [SerializeField] private ReviveMenu reviveRewardMenu;

    [SerializeField] private int maxRevivesCount = 3;
    public int MaxRevivesCount => maxRevivesCount;

    [SerializeField] private float restoreReviveTime = 300f;
    private float currentRestoreReviveTime = 0f;

    public int RemainingRevivesCount { get; private set; } = 0;

    public event Action onRevivesCountChanged;

    private void OnEnable()
    {
        SelectManager.onComponentSelected += OnComponentSelected;
    }

    private void OnDisable()
    {
        SelectManager.onComponentSelected -= OnComponentSelected;
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

    public void RemoveReviveCount()
    {
        SetRevivesCount(RemainingRevivesCount - 1);
    }

    private void SetRevivesCount(int value)
    {
        if (RemainingRevivesCount == value) return;
        int lastCount = RemainingRevivesCount;

        RemainingRevivesCount = value;
        onRevivesCountChanged?.Invoke();
    }

    private void AddReviveCount()
    {
        SetRevivesCount(RemainingRevivesCount + 1);
    }

    private void ResetRestoreReviveTime()
    {
        currentRestoreReviveTime = 0f;
    }

    private void CreateReward(Human human)
    {
        float time = human.ReviveComponent.ReviveLimitTime - human.ReviveComponent.CurrentDiedTime;
        ReviveAdRewardInstance reward = reviveRewardDefinition.CreateReward() as ReviveAdRewardInstance;
        reward.SetHuman(human);

        RewardedAdsManager.Instance.SetCurrentReward(reward);
    }

    private void OnComponentSelected(SelectComponent component)
    {
        Human human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return;

        if (human.HealthComponent.IsAlive) return;

        CreateReward(human);
        reviveRewardMenu.Open();
    }
}