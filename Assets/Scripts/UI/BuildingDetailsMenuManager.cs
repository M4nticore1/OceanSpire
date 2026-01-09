//using TMPro;
//using Unity.Mathematics;
//using UnityEngine;
//using UnityEngine.UI;

//public class BuildingDetailsMenuManager : MonoBehaviour
//{
//    private UIManager uiManager = null;

//    // General
//    [Header("Building")]
//    [SerializeField] private TextMeshProUGUI nameText = null;
//    [SerializeField] private TextMeshProUGUI healthValueText = null;
//    [SerializeField] private TextMeshProUGUI levelNumberText = null;
//    [SerializeField] private Button closeMenuButton = null;

//    // Building
//    [Header("Building")]
//    [SerializeField] private GameObject buildingMenu = null;
//    [SerializeField] private Button buildingUpgradeButton = null;
//    [SerializeField] private Button buildingDemolishButton = null;

//    // Production
//    [Header("Production Building")]
//    [SerializeField] private GameObject productionBuildingMenu = null;
//    [SerializeField] private Button productionUpgradeButton = null;
//    [SerializeField] private Button productionDemolishButton = null;
//    [SerializeField] private Button productionWorkersMenuButton = null;

//    // Storage
//    [Header("Storage Building")]
//    [SerializeField] private GameObject storageBuildingMenu = null;
//    [SerializeField] private Button storageUpgradeButton = null;
//    [SerializeField] private Button storageDemolishButton = null;
//    [SerializeField] private Button storageWorkersMenuButton = null;
//    [SerializeField] private Button storageLootMenuButton = null;

//    // Boat
//    [Header("Boat")]
//    [SerializeField] private GameObject boatMenu = null;
//    [SerializeField] private TextMeshProUGUI boatWeightValueText = null;
//    [SerializeField] private GridLayoutGroup boatLootLayoutGroup = null;

//    private void Awake()
//    {
//        uiManager = GetComponentInParent<UIManager>();
//        // Building
//        buildingUpgradeButton.onClick.AddListener(uiManager.OpenUpgradeBuildingMenu);
//        buildingDemolishButton.onClick.AddListener(uiManager.OpenDemolishBuildingMenu);
//        // Production
//        productionUpgradeButton.onClick.AddListener(uiManager.OpenUpgradeBuildingMenu);
//        productionDemolishButton.onClick.AddListener(uiManager.OpenDemolishBuildingMenu);
//        productionWorkersMenuButton.onClick.AddListener(uiManager.OpenBuildingWorkersMenu);
//        // Storage
//        storageUpgradeButton.onClick.AddListener(uiManager.OpenUpgradeBuildingMenu);
//        storageDemolishButton.onClick.AddListener(uiManager.OpenDemolishBuildingMenu);
//        storageWorkersMenuButton.onClick.AddListener(uiManager.OpenBuildingWorkersMenu);
//        //storageLootMenuButton.onClick.AddListener(uiManager.OpenUpgradeBuildingMenu);
//    }

//    public void InitializeBuildingDetailsMenu(GameObject objectToShowDetails)
//    {
//        buildingMenu.SetActive(false);
//        productionBuildingMenu.SetActive(false);
//        storageBuildingMenu.SetActive(false);
//        boatMenu.SetActive(false);
//        healthValueText.gameObject.SetActive(false);
//        levelNumberText.gameObject.SetActive(false);

//        Building building = objectToShowDetails.GetComponent<Building>();
//        Entity entity = objectToShowDetails.GetComponent<Entity>();
//        Boat boat = objectToShowDetails.GetComponent<Boat>();

//        if (building)
//        {
//            levelNumberText.gameObject.SetActive(true);

//            SetNameText(building.BuildingData.BuildingName);
//            SetLevelText(building.levelIndex + 1);

//            ProductionBuilding productionBuilding = building.productionComponent;
//            StorageBuildingComponent storageBuilding = building.storageComponent;
//            if (productionBuilding) {
//                productionBuildingMenu.SetActive(true);
//            }
//            else if (storageBuilding) {
//                storageBuildingMenu.SetActive(true);
//            }
//            else {
//                buildingMenu.SetActive(true);
//            }
//        }
//        else if (entity)
//        {
//            SetNameText(entity.firstName + " " + entity.lastName);
//        }
//        else if (boat)
//        {
//            boatMenu.SetActive(true);

//            SetNameText(boat.BoatData.BoatName);
//            SetHealthValue(boat.currentHealth, boat.BoatData.MaxHealth);
//            SetBoatCurrentWeight(boat.currentWeight, boat.BoatData.MaxWeight);
//        }
//    }

//    private void SetNameText(string name)
//    {
//        nameText.SetText(name);
//    }

//    public void SetHealthValue(float currentHealth, float maxHealth)
//    {
//        healthValueText.SetText(math.floor(currentHealth) + "/" + math.floor(maxHealth));
//    }

//    private void SetLevelText(int levelNumber)
//    {
//        levelNumberText.SetText("Level " + levelNumber.ToString());
//    }

//    public void SetBoatCurrentWeight(float currentWeight, float maxWeight)
//    {
//        boatWeightValueText.SetText("Weight\n" + (int)currentWeight + "/" + (int)maxWeight);
//    }
//}
