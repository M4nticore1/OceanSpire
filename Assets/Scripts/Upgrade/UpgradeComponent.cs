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
    public static event Action<UpgradeComponent> OnGlobalUpgradeCompleted;

    private void Awake()
    {
        upgradable = GetComponent<IUpgradable>();

        if (upgradable == null) {
            Debug.Log($"Upgrade component is on the game object {gameObject} that doesn't have any component with an IUpgradable interface");
        }
    }

    private void OnEnable()
    {
        constructionComponent.OnConstructionCompleted += OnConstructionCompleted;
    }

    private void OnDisable()
    {
        constructionComponent.OnConstructionCompleted -= OnConstructionCompleted;
    }

    public void Init(UpgradeData upgradeData)
    {
        NextLevel = upgradeData.NextLevel;

        if (upgradeData.UnderUpgrade) {
            StartUpgrading();
        }
    }

    public void StartUpgrading()
    {
        IsUnderUpgrade = true;
        NextLevel = levelComponent.Level + 1;
        constructionComponent.StartConstruction(upgradable.GetUpgradeTime());

        OnUpgradeStarted?.Invoke();
        OnGlobalUpgradeStarted?.Invoke(this);
    }

    private void OnConstructionCompleted()
    {
        IsUnderUpgrade = false;
        levelComponent.TrySetLevel(NextLevel);

        OnUpgradeFinished?.Invoke();
        OnGlobalUpgradeCompleted?.Invoke(this);
    }
}