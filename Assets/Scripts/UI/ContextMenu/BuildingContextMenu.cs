using TMPro;
using UnityEngine;

public class BuildingContextMenu : ContextMenuBase<Building>
{
    [SerializeField] private TextMeshProUGUI levelNumberText = null;

    [SerializeField] private CustomButton upgradeButton = null;
    [SerializeField] private CustomButton demolishButton = null;
    [SerializeField] private CustomButton workersButton = null;
    [SerializeField] private CustomButton storageButton = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (upgradeButton)
            upgradeButton.onReleased += EventBus.InvokeContextMenuUpgradeButtonClicked;
        if (demolishButton)
            demolishButton.onReleased += EventBus.InvokeContextMenuDemolishButtonClicked;
        if (workersButton)
            workersButton.onReleased += EventBus.InvokeContextMenuWorkersButtonClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (upgradeButton)
            upgradeButton.onReleased -= EventBus.InvokeContextMenuUpgradeButtonClicked;
        if (demolishButton)
            demolishButton.onReleased -= EventBus.InvokeContextMenuDemolishButtonClicked;
        if (workersButton)
            workersButton.onReleased -= EventBus.InvokeContextMenuWorkersButtonClicked;
    }

    public override void Init(Building building)
    {
        SetNameText(building.BuildingData.BuildingName);
        SetLevelText(building.LevelIndex + 1);
    }

    private void SetLevelText(int levelNumber)
    {
        levelNumberText.SetText("Level " + levelNumber.ToString());
    }
}