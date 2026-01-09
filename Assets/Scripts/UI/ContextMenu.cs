using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class ContextMenu : MonoBehaviour
{
    private UIManager uiManager = null;

    [Header("Main")]
    [SerializeField] private TextMeshProUGUI nameText = null;

    [Header("Custom")]
    [SerializeField] private TextMeshProUGUI healthValueText = null;
    [SerializeField] private TextMeshProUGUI levelNumberText = null;
    [SerializeField] private Button upgradeButton = null;
    [SerializeField] private Button demolishButton = null;
    [SerializeField] private Button workersButton = null;
    [SerializeField] private Button storageButton = null;

    [Header("Boat")]
    [SerializeField] private TextMeshProUGUI boatWeightValueText = null;

    private void Awake()
    {
        uiManager = GetComponentInParent<UIManager>();
        if (upgradeButton)
            upgradeButton.onClick.AddListener(uiManager.OpenUpgradeBuildingMenu);
        if (demolishButton)
            demolishButton.onClick.AddListener(uiManager.OpenDemolishBuildingMenu);
        if (workersButton)
            workersButton.onClick.AddListener(uiManager.OpenBuildingWorkersMenu);
    }

    public void Open(GameObject objectToShowDetails)
    {
        Building building = objectToShowDetails.GetComponent<Building>();
        Entity entity = objectToShowDetails.GetComponent<Entity>();
        Boat boat = objectToShowDetails.GetComponent<Boat>();

        if (building) {
            SetNameText(building.BuildingData.BuildingName);
            SetLevelText(building.levelIndex + 1);
        }
        else if (entity) {
            SetNameText(entity.firstName + " " + entity.lastName);
        }
        else if (boat) {
            SetNameText(boat.BoatData.BoatName);
            SetHealthValue(boat.currentHealth, boat.BoatData.MaxHealth);
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
