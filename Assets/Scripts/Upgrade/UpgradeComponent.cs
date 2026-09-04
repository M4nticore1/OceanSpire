using System;
using UnityEngine;

public class UpgradeComponent : MonoBehaviour
{
    [SerializeField] private LevelComponent levelComponent;
    [SerializeField] private ConstructionComponent constructionComponent;

    private IUpgradable upgradable;
    public bool IsUnderUpgrade { get; private set; } = false;
    public int NextLevel { get; private set; } = 1;

    public event Action OnUpgradeStarted;
    public event Action OnUpgradeFinished;

    public static event Action<UpgradeComponent> OnGlobalUpgradeStarted;
    public static event Action<UpgradeComponent> OnGlobalUpgradeFinished;

    private void Awake()
    {
        upgradable = GetComponent<IUpgradable>();
        if (upgradable == null) {
            Debug.Log($"Upgrade component is on the game object {gameObject} that doesn't have any component with an IUpgradable interface");
        }
    }

    private void OnEnable()
    {
        constructionComponent.OnConstructionFinished += OnConstructionCompleted;
    }

    private void OnDisable()
    {
        constructionComponent.OnConstructionFinished -= OnConstructionCompleted;
    }

    public void Init()
    {
        Init(UpgradeData.Default());
    }

    public void Init(UpgradeData upgradeData)
    {
        if (upgradeData == null) {
            Debug.LogError($"[{nameof(UpgradeComponent)}] Upgrade Data is not valid!");
            upgradeData = UpgradeData.Default();
        }

        NextLevel = upgradeData.NextLevel;

        if (upgradeData.UnderUpgrade) {
            IsUnderUpgrade = true;

            OnUpgradeStarted?.Invoke();
            OnGlobalUpgradeStarted?.Invoke(this);
        }
    }

    public void StartUpgrading()
    {
        if (upgradable == null) return;
        if (levelComponent == null) return;
        if (constructionComponent == null) return;

        IsUnderUpgrade = true;
        NextLevel = levelComponent.Level + 1;
        constructionComponent.StartConstruction(upgradable.GetUpgradeTime());

        OnUpgradeStarted?.Invoke();
        OnGlobalUpgradeStarted?.Invoke(this);
    }

    private void OnConstructionCompleted()
    {
        if (!IsUnderUpgrade) return;

        IsUnderUpgrade = false;
        levelComponent.TrySetLevel(NextLevel);

        OnUpgradeFinished?.Invoke();
        OnGlobalUpgradeFinished?.Invoke(this);
    }
}