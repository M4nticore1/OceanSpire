using TMPro;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.EventSystems;

public class ContextMenu : UIBehaviour
{
    [Header("Main")]
    [SerializeField] private TextMeshProUGUI nameText = null;

    [Header("Custom")]
    [SerializeField] private TextMeshProUGUI healthValueText = null;
    [SerializeField] private TextMeshProUGUI levelNumberText = null;
    [SerializeField] private CustomButton upgradeButton = null;
    [SerializeField] private CustomButton demolishButton = null;
    [SerializeField] private CustomButton workersButton = null;
    [SerializeField] private CustomButton storageButton = null;

    [Header("Boat")]
    [SerializeField] private TextMeshProUGUI boatWeightValueText = null;


    protected override void OnEnable()
    {
        base.OnEnable();

        if (upgradeButton)
            upgradeButton.onReleased += EventBus.Instance.InvokeContextMenuUpgradeButtonClicked;
        if (demolishButton)
            demolishButton.onReleased += EventBus.Instance.InvokeContextMenuDemolishButtonClicked;
        if (workersButton)
            workersButton.onReleased += EventBus.Instance.InvokeContextMenuWorkersButtonClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (upgradeButton)
            upgradeButton.onReleased -= EventBus.Instance.InvokeContextMenuUpgradeButtonClicked;
        if (demolishButton)
            demolishButton.onReleased -= EventBus.Instance.InvokeContextMenuDemolishButtonClicked;
        if (workersButton)
            workersButton.onReleased -= EventBus.Instance.InvokeContextMenuWorkersButtonClicked;
    }

    public void Open(SelectComponent selectComponent)
    {
        Building building = selectComponent.GetComponent<Building>();
        Creature entity = selectComponent.GetComponent<Creature>();
        Boat boat = selectComponent.GetComponent<Boat>();

        if (building) {
            SetNameText(building.BuildingData.BuildingName);
            SetLevelText(building.LevelIndex + 1);
        }
        else if (entity) {
            SetNameText(entity.firstName + " " + entity.lastName);
        }
        else if (boat) {
            SetNameText(boat.BoatData.BoatName);
            SetHealthValue(boat.CurrentHealth, boat.BoatData.MaxHealth);
            SetBoatCurrentWeight(boat.currentWeight, boat.BoatData.MaxWeight);
        }
    }

    private void SetNameText(string name)
    {
        nameText.SetText(name);
    }

    public void SetHealthValue(float currentHealth, float maxHealth)
    {
        healthValueText.SetText(math.floor(currentHealth) + "/" + math.floor(maxHealth));
    }

    private void SetLevelText(int levelNumber)
    {
        levelNumberText.SetText("Level " + levelNumber.ToString());
    }

    public void SetBoatCurrentWeight(float currentWeight, float maxWeight)
    {
        boatWeightValueText.SetText("Weight\n" + (int)currentWeight + "/" + (int)maxWeight);
    }
}
