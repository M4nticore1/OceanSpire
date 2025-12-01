using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class DetailsMenu : MonoBehaviour
{
    private UIManager uiManager = null;

    // General
    [Header("General")]
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private TextMeshProUGUI healthValueText = null;
    [SerializeField] private TextMeshProUGUI levelNumberText = null;
    [SerializeField] private Button upgradeButton = null;
    [SerializeField] private Button demolishButton = null;
    [SerializeField] private Button closeDetailsMenuButton = null;

    // Building
    [Header("Building")]
    [SerializeField] private Button openWorkersMenuButton = null;

    // Boat
    [Header("Boat")]
    [SerializeField] private GameObject boatMenu = null;
    [SerializeField] private TextMeshProUGUI currentWeightText = null;
    [SerializeField] private GridLayoutGroup currentResourcesLayoutGroup = null;

    public void Initialize(Building building, UIManager uiManager)
    {
        Initialize_Internal(uiManager);

        nameText.gameObject.SetActive(true);
        levelNumberText.gameObject.SetActive(true);
        upgradeButton.gameObject.SetActive(true);
        demolishButton.gameObject.SetActive(true);
        openWorkersMenuButton.gameObject.SetActive(true);

        SetNameText(building.BuildingData.BuildingName);
        SetLevelText(building.levelComponent.LevelIndex + 1);

        if (building.BuildingData.IsDemolishable)
            demolishButton.interactable = true;
        else
            demolishButton.interactable = false;

        if (building.levelComponent.LevelIndex < building.ConstructionLevelsData.Count + 1)
            upgradeButton.interactable = true;
        else
            upgradeButton.interactable = false;
    }

    public void Initialize(Boat boat, UIManager uiManager)
    {
        Initialize_Internal(uiManager);

        boat.spawnedDetailsMenu = this;

        boatMenu.SetActive(true);
        nameText.gameObject.SetActive(true);
        currentWeightText.gameObject.SetActive(true);

        SetNameText(boat.BoatData.BoatName);
        SetBoatCurrentWeight(boat.currentWeight, boat.BoatData.MaxWeight);
    }

    public void Initialize(Entity entity, UIManager uiManager)
    {
        Initialize_Internal(uiManager);

        nameText.gameObject.SetActive(true);

        SetNameText(entity.firstName + " " + entity.lastName);
    }

    private void Initialize_Internal(UIManager uiManager)
    {
        if (!uiManager) {
            Debug.Log("uiManager is NULL");
            return; }

        this.uiManager = uiManager;

        boatMenu.SetActive(false);
        nameText.gameObject.SetActive(false);
        levelNumberText.gameObject.SetActive(false);
        currentWeightText.gameObject.SetActive(false);
        currentResourcesLayoutGroup.gameObject.SetActive(false);

        upgradeButton.onClick.AddListener(uiManager.OpenUpgradeBuildingMenu);
        upgradeButton.gameObject.SetActive(false);

        openWorkersMenuButton.onClick.AddListener(uiManager.OpenBuildingWorkersMenu);
        openWorkersMenuButton.gameObject.SetActive(false);

        closeDetailsMenuButton.onClick.AddListener(uiManager.CloseDetailsMenu);
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
        currentWeightText.SetText("Weight\n" + (int)currentWeight + "/" + (int)maxWeight);
    }
}
