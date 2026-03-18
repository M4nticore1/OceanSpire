using TMPro;
using UnityEngine;

public class BuildingContextMenu : ContextMenu<Building>
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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (spawnedUpgradeButton) {
            spawnedUpgradeButton.onReleased -= EventBus.InvokeUpgradeButtonClicked;
        }
        if (spawnedDemolishButton) {
            spawnedDemolishButton.onReleased -= EventBus.InvokeDemolishButtonClicked;
        }
    }

    public override void Init(Building building)
    {
        if (!building) {
            Debug.LogWarning("building is not on the scene.");
            return;
        }

        SetNameLocalization(building.BuildingData.LocalizationItem);
        SetLevelText(building.LevelIndex + 1);

        if (building.GetComponent<ProductionModule>() || building.GetComponent<PierModule>()) {
            CreateWorkersButton();
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
    }

    private void CreateStorageButton()
    {
        spawnedStorageButton = CreateButton(storageButton);
    }

    private void CreateUpgradeButton()
    {
        spawnedUpgradeButton = CreateButton(upgradeButton);
        //spawnedUpgradeButton.onReleased += EventBus.InvokeUpgradeButtonClicked;
    }

    private void CreateDemolishButton()
    {
        spawnedDemolishButton = CreateButton(demolishButton);
        //spawnedDemolishButton.onReleased += EventBus.InvokeDemolishButtonClicked;
    }

    private CustomButton CreateButton(CustomButton button)
    {
        return CreatePanel(button.gameObject).GetComponent<CustomButton>();
    }
}