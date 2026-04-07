using TMPro;
using UnityEngine;

public class BuildingContextMenu : ContextMenu<Building>
{
    [SerializeField] private TextMeshProUGUI levelNumberText = null;

    [SerializeField] private CustomButton workersButton = null;
    [SerializeField] private CustomButton productionButton = null;
    [SerializeField] private CustomButton storageButton = null;
    [SerializeField] private CustomButton upgradeButton = null;
    [SerializeField] private CustomButton demolishButton = null;

    private CustomButton spawnedWorkersButton = null;
    private CustomButton spawnedProductionButton = null;
    private CustomButton spawnedStorageButton = null;
    private CustomButton spawnedUpgradeButton = null;
    private CustomButton spawnedDemolishButton = null;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (spawnedWorkersButton) {
            spawnedWorkersButton.onReleased -= EventBus.InvokeContextWorkersButtonClicked;
        }
        if (spawnedProductionButton) {
            spawnedProductionButton.onReleased -= EventBus.InvokeContextProductionButtonClicked;
        }
        if (spawnedUpgradeButton) {
            spawnedUpgradeButton.onReleased -= EventBus.InvokeContextUpgradeButtonClicked;
        }
        if (spawnedDemolishButton) {
            spawnedDemolishButton.onReleased -= EventBus.InvokeContextDemolishButtonClicked;
        }
    }

    public override void Init(Building building)
    {
        if (!building) {
            Debug.LogWarning("building is not on the scene.");
            return;
        }

        Debug.Log("InitContextMenu");

        SetNameLocalization(building.BuildingData.LocalizationItem);
        SetLevelText(building.LevelIndex + 1);

        if (building.GetComponent<ProductionModule>() || building.GetComponent<PierModule>()) {
            CreateWorkersButton();
        }

        if (building.GetComponent<ProductionModule>()) {
            CreateProductionButton();
        }

        if (building.GetComponent<StorageBuildingModule>()) {
            CreateStorageButton();
        }

        CreateUpgradeButton();

        if (building.BuildingData.IsDemolishable) {
            CreateDemolishButton();
        }

        if (spawnedUpgradeButton && !building.NextLevelData) {
            spawnedUpgradeButton.SetState(CustomButtonState.Disabled);
            spawnedUpgradeButton.FinishTransitionAnimation();
        }
    }

    private void SetLevelText(int levelNumber)
    {
        levelNumberText.SetText("Level " + levelNumber.ToString());
    }

    private void CreateWorkersButton()
    {
        spawnedWorkersButton = CreateButton(workersButton);
        spawnedWorkersButton.onReleased += EventBus.InvokeContextWorkersButtonClicked;
    }

    private void CreateProductionButton()
    {
        spawnedProductionButton = CreateButton(productionButton);
        spawnedProductionButton.onReleased += EventBus.InvokeContextProductionButtonClicked;
    }

    private void CreateStorageButton()
    {
        spawnedStorageButton = CreateButton(storageButton);
    }

    private void CreateUpgradeButton()
    {
        spawnedUpgradeButton = CreateButton(upgradeButton);
        spawnedUpgradeButton.onReleased += EventBus.InvokeContextUpgradeButtonClicked;
    }

    private void CreateDemolishButton()
    {
        spawnedDemolishButton = CreateButton(demolishButton);
        spawnedDemolishButton.onReleased += EventBus.InvokeContextDemolishButtonClicked;
    }

    private CustomButton CreateButton(CustomButton button)
    {
        return CreatePanel(button.gameObject).GetComponent<CustomButton>();
    }
}