using TMPro;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.EventSystems;

public class ContextMenuUI : UIBehaviour
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

    public void Init(Building building)
    {
        SetNameText(building.BuildingData.BuildingName);
        SetLevelText(building.LevelIndex + 1);
    }

    public void Init(Human human)
    {
        SetNameText(human.firstName + " " + human.lastName);
    }

    public void Init(Boat boat)
    {
        SetNameText(boat.BoatData.BoatName);
        SetHealthValue(boat.CurrentHealth, boat.MaxHealth);
        SetBoatCurrentWeight(boat.CurrentWeight, boat.MaxWeight);
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
