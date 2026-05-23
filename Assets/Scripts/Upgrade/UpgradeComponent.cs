using UnityEngine;

public class UpgradeComponent : MonoBehaviour
{
    [SerializeField] private LevelComponent levelComponent;
    [SerializeField] private ConstructionComponent constructionComponent;

    private IUpgradable upgradable;
    public int NextLevel { get; private set; } = 1;

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
    }

    public void Upgrade()
    {
        NextLevel = levelComponent.Level + 1;
        constructionComponent.StartConstruction(upgradable.GetUpgradeTime());
    }

    private void OnConstructionCompleted()
    {
        levelComponent.SetLevel(NextLevel);
    }
}