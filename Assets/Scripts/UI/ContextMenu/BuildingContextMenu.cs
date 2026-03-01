using TMPro;
using UnityEngine;

public class BuildingContextMenu : ContextMenuBase<Building>
{
    [SerializeField] private TextMeshProUGUI levelNumberText = null;

    [SerializeField] private CustomButton workersButton = null;
    [SerializeField] private CustomButton storageButton = null;
    [SerializeField] private CustomButton upgradeButton = null;
    [SerializeField] private CustomButton demolishButton = null;

    private CustomButton spawnedWorkersButton = null;
    private CustomButton spawnedStorageButton = null;
    private CustomButton spawnedUpgradeButton = null;
    private CustomButton spawnedDemolishButton = null;

    protected override void OnDisable()
    {
        base.OnDisable();

        if (spawnedWorkersButton) {
            spawnedWorkersButton.onReleased -= EventBus.InvokeContextMenuWorkersButtonClicked;
        }
        if (spawnedUpgradeButton) {
            spawnedUpgradeButton.onReleased -= EventBus.InvokeContextMenuUpgradeButtonClicked;
        }
        if (spawnedDemolishButton) {
            spawnedDemolishButton.onReleased -= EventBus.InvokeContextMenuDemolishButtonClicked;
        }
    }

    public override void Init(Building building)
    {
        SetNameText(building.BuildingData.BuildingName);
        SetLevelText(building.LevelIndex + 1);

        if (building.GetComponent<ProductionModule>()) {
            CreateWorkersButton();
        }

        if (building.GetComponent<StorageBuildingModule>()) {
            CreateStorageButton();
        }

        CreateUpgradeButton();

        if (building.BuildingData.IsDemolishable) {
            CreateDemolishButton();
        }

        if (upgradeButton && !building.NextLevelData) {
            upgradeButton.SetState(CustomSelectableState.Disabled);
            upgradeButton.FinishTransitionAnimation();
        }
    }

    private void SetLevelText(int levelNumber)
    {
        levelNumberText.SetText("Level " + levelNumber.ToString());
    }

    private void CreateWorkersButton()
    {
        spawnedWorkersButton = CreateButton(workersButton);
        spawnedWorkersButton.onReleased += EventBus.InvokeContextMenuWorkersButtonClicked;
    }

    private void CreateStorageButton()
    {
        spawnedStorageButton = CreateButton(storageButton);
    }

    private void CreateUpgradeButton()
    {
        spawnedUpgradeButton = CreateButton(upgradeButton);
        spawnedUpgradeButton.onReleased += EventBus.InvokeContextMenuUpgradeButtonClicked;
    }

    private void CreateDemolishButton()
    {
        spawnedDemolishButton = CreateButton(demolishButton);
        spawnedDemolishButton.onReleased += EventBus.InvokeContextMenuDemolishButtonClicked;
    }

    private CustomButton CreateButton(CustomButton button)
    {
        return CreatePanel(button.gameObject).GetComponent<CustomButton>();
    }
}