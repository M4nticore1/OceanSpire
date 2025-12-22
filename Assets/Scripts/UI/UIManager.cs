using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UIManager : MonoBehaviour
{
    private PlayerController playerController;
    private GameManager gameManager;
    private CityManager cityManager;
    private GameObject selectedObject = null;

    // Widgets
    [Header("Widgets")]
    [SerializeField] private BuildingWidget buildingWidgetPrefab = null;
    [SerializeField] private ResourceWidget storageResourceWidgetPrefab = null;
    [SerializeField] private ResourceWidget buildingActionResourceWidgetPrefab = null;
    [SerializeField] private List<ResourceWidget> storageResourceWidgets = new List<ResourceWidget>();
    [SerializeField] private BuildingCharacteristicWidget buildingCharacteristicWidget = null;

    private List<List<BuildingWidget>> spawnedBuildingWidgets = new List<List<BuildingWidget>>();
    private List<ResourceWidget> spawnedBuildingActionResourceWidgets = new List<ResourceWidget>();
    private List<BuildingCharacteristicWidget> spawnedBuildingCharacteristicWidgets = new List<BuildingCharacteristicWidget>();

    // Menus
    [Header("Menus")]
    [SerializeField] private GameObject managementMenu = null;
    [SerializeField] private GameObject buildingListsMenu = null;
    [SerializeField] private GameObject storageListsMenu = null;
    private bool isManagementMenuOpened = false;
    private bool isBuildingListsMenuOpened = false;
    private bool isStorageListsMenuOpened = false;
    private bool isDetailsMenuOpened = false;

    BuildingCategory lastOpenedBuildingsListCategory = BuildingCategory.Construction;
    ItemCategory lastOpenedStorageListCategory = ItemCategory.Building;

    [Header("Building Information Menu")]
    [SerializeField] private GameObject buildingInformationMenu = null;
    [SerializeField] private TextMeshProUGUI buildingInformationMenuNameText = null;
    [SerializeField] private TextMeshProUGUI buildingInformationMenuLevelNumberText = null;
    [SerializeField] private TextMeshProUGUI buildingInformationMenuDescriptionText = null;
    [SerializeField] private RectTransform buildingCharacteristics = null;
    [SerializeField] private Button closeBuildingInformationMenuButton = null;
    [SerializeField] private Button closeBuildingInformationMenuBackgroundButton = null;
    private float buildingCharacteristicHeight = 0;

    // Menu Buttons
    [Header("Menu Buttons")]
    [SerializeField] private Button buildingMenuButton = null;
    [SerializeField] private Button storageMenuButton = null;
    [SerializeField] private MainButton buildingListsMenuButton = null;
    [SerializeField] private MainButton storageListsMenuButton = null;
    [SerializeField] private Button closeManagementMenuButton = null;
    [SerializeField] private Button stopPlacingBuildingButton = null;

    // Buildings
    [Header("Building Lists")]
    [SerializeField] private List<GridLayoutGroup> buildingLists = new List<GridLayoutGroup>();
    [SerializeField] private List<MainButton> buildingListButtons = new List<MainButton>();
    [SerializeField] private ScrollRect buildingListsScrollRect = null;
    [SerializeField] private ScrollRect storageListsScrollRect = null;

    // Storage List
    [Header("Storage Lists")]
    [SerializeField] private List<GridLayoutGroup> storageLists = new List<GridLayoutGroup>();
    [SerializeField] private List<MainButton> storageListButtons = new List<MainButton>();

    private List<bool> itemsToUpdate = new List<bool>();

    // Building Management Menu
    [Header("Building Management Menu")]
    [SerializeField] private RectTransform buildingManagementMenu = null;
    [SerializeField] private Button closeBuildingWorkersMenuButton = null;
    private Vector2 buildingManagementMenuCurrentPosition = Vector2.zero;
    private const float buildingManagementMenuToggleSpeed = 15.0f;
    [SerializeField] private DetailsMenu detailsMenu = null;

    [Header("Building Stats Panel")]
    [SerializeField] private RectTransform buildingStatsPanel = null;
    [SerializeField] private TextMeshProUGUI buildingStatsPanelNameText = null;
    [SerializeField] private TextMeshProUGUI buildingStatsPanelWorkersCountText = null;
    private bool isInfoMenuOpened = false;
    private Vector2 buildingStatsPanelOpenedPosition = Vector2.zero;

    [Header("Building Action Menu")]
    [SerializeField] private RectTransform buildingResourcesMenuPanel = null;
    [SerializeField] private RectTransform buildingResourcesMenuBackground = null;
    [SerializeField] private Button closeBuildingResourcesMenuBackgroundButton = null;
    [SerializeField] private Button closeBuildingResourcesMenuButton = null;
    [SerializeField] private GridLayoutGroup actionResourcesLayourGroup = null;
    private const float buildingResourcesMenuPanelToggleSpeed = 10.0f;
    [HideInInspector] public bool isBuildingResourcesMenuOpened = false;
    private Vector2 buildingResourcesMenuOpenedPosition = Vector2.zero;
    private Vector2 buildingResourcesMenuClosedPosition = Vector2.zero;
    private Vector2 buildingResourcesMenuCurrentPosition = Vector2.zero;

    [Header("Upgrade Menu")]
    [SerializeField] private RectTransform upgradeBuildingMenu = null;
    [SerializeField] private Button upgradeBuildingButton = null;

    [Header("Demolish Menu")]
    [SerializeField] private RectTransform demolishBuildingMenu = null;
    [SerializeField] private Button demolishBuildingButton = null;

    [Header("Repair Menu")]
    [SerializeField] private RectTransform repairBuildingMenu = null;
    [SerializeField] private Button repairBuildingButton = null;
    [SerializeField] private TextMeshProUGUI repairBuildingNameText = null;

    [Header("Building Workers Menu")]
    [SerializeField] private ResidentWidget residentWidgetPrefab = null;

    private List<ResidentWidget> spawnedBuildingWorkerEmptyWidgets = new List<ResidentWidget>();
    private List<ResidentWidget> spawnedResidentWidgets = new List<ResidentWidget>();

    [SerializeField] private RectTransform selectBuildingWorkersMenu = null;
    [SerializeField] private RectTransform buildingWorkersMenu = null;
    [SerializeField] private RectTransform unemployedResidentsMenu = null;
    [SerializeField] private RectTransform employedResidentsMenu = null;

    [SerializeField] private RectTransform haveNoUnemployedResidentsText = null;
    [SerializeField] private RectTransform haveNoEmployedResidentsText = null;

    [SerializeField] private GridLayoutGroup buildingWorkersList = null;
    [SerializeField] private GridLayoutGroup unemployedResidentsList = null;
    [SerializeField] private GridLayoutGroup employedResidentsList = null;

    [Header("FPS")]
    [SerializeField] private TextMeshProUGUI fpsText = null;

    int maxBuildingWorkersCount = 0;
    int residentWidgetsColumnCount = 0;

    int frames = 0;
    double lastFPSCounterTime = 0d;

    // Colors
    [Header("Colors")]
    [SerializeField] private UIColor blueColor;
    [SerializeField] private UIColor lightBlueColor;
    [SerializeField] private UIColor darkBlueColor;

    public static event Action OnBuildStopPlacing;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    //private void Start()
    //{
    //    InitializeUIManager();
    //}

    private void OnEnable()
    {
        BuildingWidget.OnStartPlacingConstruction += OnConstructionStartPlacing;
        CityManager.OnConstructionPlaced += OnConstructionPlaced;
        CityManager.OnStorageCapacityUpdated += UpdateItemAmounts;
        CityManager.OnLootAdded += UpdateItemAmounts;
        if (cityManager)
        {
            cityManager.OnResidentAdded += AddResidentWidget;
        }
        else
            Debug.LogError("cityManager is NULL");
    }

    private void OnDisable()
    {
        CityManager.OnStorageCapacityUpdated -= UpdateItemAmounts;
        CityManager.OnLootAdded -= UpdateItemAmounts;
        if (cityManager)
        {
            cityManager.OnResidentAdded -= AddResidentWidget;
        }
        else
            Debug.LogError("cityManager is NULL");

        BuildingWidget.OnStartPlacingConstruction -= OnConstructionStartPlacing;
    }

    private void Update()
    {
        // Building Management Menu
        if (isDetailsMenuOpened)
            buildingManagementMenuCurrentPosition.y = math.lerp(buildingManagementMenuCurrentPosition.y, buildingManagementMenu.rect.size.y, buildingManagementMenuToggleSpeed * Time.deltaTime);
        else
            buildingManagementMenuCurrentPosition.y = math.lerp(buildingManagementMenuCurrentPosition.y, 0, buildingManagementMenuToggleSpeed * Time.deltaTime);

		buildingManagementMenu.anchoredPosition = buildingManagementMenuCurrentPosition;

        if (isBuildingResourcesMenuOpened)
            buildingResourcesMenuCurrentPosition.y = math.lerp(buildingResourcesMenuCurrentPosition.y, buildingResourcesMenuOpenedPosition.y, buildingResourcesMenuPanelToggleSpeed * Time.deltaTime);
        else
            buildingResourcesMenuCurrentPosition.y = math.lerp(buildingResourcesMenuCurrentPosition.y, 0, buildingResourcesMenuPanelToggleSpeed * Time.deltaTime);

        buildingResourcesMenuPanel.anchoredPosition = buildingResourcesMenuCurrentPosition;

        UpdateBuildingStatsPanelPosition();
        FPSCounter();
    }

    public void InitializeUIManager()
    {
        CreateBuildingWidgets();
        CreateItemWidgets();

        buildingResourcesMenuOpenedPosition.y = Screen.height / 2 + (buildingResourcesMenuPanel.rect.size.y / 2);
        buildingResourcesMenuCurrentPosition.y = 0;

        buildingStatsPanelOpenedPosition = buildingStatsPanel.anchoredPosition;
        buildingStatsPanel.anchoredPosition = Vector2.zero;

        buildingMenuButton.onClick.AddListener(OpenConstructionMenu);
        storageMenuButton.onClick.AddListener(OpenStorageMenu);

        buildingListsMenuButton.onClick.AddListener(OpenConstructionListsMenu);
        storageListsMenuButton.onClick.AddListener(OpenStorageListsMenu);

        closeManagementMenuButton.onClick.AddListener(CloseManagementMenu);

        stopPlacingBuildingButton.onClick.AddListener(StopPlacingBuilding);

        closeBuildingInformationMenuButton.onClick.AddListener(CloseBuildingInformationMenu);
        closeBuildingInformationMenuBackgroundButton.onClick.AddListener(CloseBuildingInformationMenu);

        // Building List Buttons
        System.Array buildingCategoriesEnum = System.Enum.GetValues(typeof(BuildingCategory));
        for (int i = 0; i < buildingCategoriesEnum.Length; i++)
        {
            int index = i;
            buildingListButtons[index].onClick.AddListener(() => OpenBuildingsListByCategory((BuildingCategory)buildingCategoriesEnum.GetValue(index)));
        }

        // Storage List Buttons
        System.Array itemCategoriesEnum = System.Enum.GetValues(typeof(ItemCategory));
        for (int i = 0; i < storageListButtons.Count; i++)
        {
            int index = i;
            storageListButtons[index].onClick.AddListener(() => OpenStorageListByCategory((ItemCategory)itemCategoriesEnum.GetValue(index + 1)));
        }

        closeBuildingResourcesMenuButton.onClick.AddListener(CloseBuildingActionMenu);
        closeBuildingResourcesMenuBackgroundButton.onClick.AddListener(CloseBuildingActionMenu);
        closeBuildingWorkersMenuButton.onClick.AddListener(CloseBuildingWorkersMenu);

        upgradeBuildingButton.onClick.AddListener(TryToUpgradeBuilding);
        demolishBuildingButton.onClick.AddListener(TryToDemolishBuilding);
        repairBuildingButton.onClick.AddListener(TryToUpgradeBuilding);

        buildingCharacteristicHeight = buildingCharacteristicWidget.characteristicValueBox.sizeDelta.y;

        for (int i = 0; i < ItemDatabase.items.Count; i++)
            itemsToUpdate.Add(false);

        managementMenu.SetActive(false);
        buildingListsMenu.SetActive(false);
        selectBuildingWorkersMenu.gameObject.SetActive(false);
        buildingInformationMenu.SetActive(false);
        stopPlacingBuildingButton.gameObject.SetActive(false);
    }

    // Management Menu
    private void CreateItemWidgets()
    {
        for (int i = 0; i < ItemDatabase.items.Count; i++)
        {
            ItemData itemData = ItemDatabase.items[i];
            if (itemData.ItemCategory == ItemCategory.Society) continue;

            ItemCategory itemCategory = itemData.ItemCategory;

            ResourceWidget storageResourceWidget = Instantiate(storageResourceWidgetPrefab, storageLists[(int)itemCategory - 1].transform);
            storageResourceWidgets.Add(storageResourceWidget);

            int itemAmount = cityManager.items[i].Amount;
            int itemMaxAmount = cityManager.totalStorageCapacity[i].Amount;
            storageResourceWidget.Initialize(itemData, itemAmount, itemMaxAmount);
        }
    }

    private void OpenManagementMenu()
    {
        isManagementMenuOpened = true;
        managementMenu.SetActive(true);
        UpdateBuildingWidgetsResourcesAmount();
    }

    public void CloseManagementMenu()
    {
        isManagementMenuOpened = false;
        managementMenu.SetActive(false);
    }

    private void OpenConstructionMenu()
    {
        OpenManagementMenu();
        OpenConstructionListsMenu();

        buildingListsMenuButton.GetComponent<RectTransform>().localScale = new Vector3(MainButton.selectedButtonUpScaleValue, MainButton.selectedButtonUpScaleValue, 1f);
        storageListsMenuButton.GetComponent<RectTransform>().localScale = Vector3.one;

        buildingListButtons[(int)lastOpenedBuildingsListCategory].GetComponent<RectTransform>().localScale = new Vector3(MainButton.selectedButtonUpScaleValue, MainButton.selectedButtonUpScaleValue, 1f);

        UpdateItemAmounts();
    }

    private void OpenConstructionListsMenu()
    {
        isBuildingListsMenuOpened = true;
        isStorageListsMenuOpened = false;

        buildingListsMenu.SetActive(true);
        storageListsMenu.SetActive(false);

        buildingListsMenuButton.Select();
        storageListsMenuButton.Deselect();

        OpenBuildingsListByCategory(lastOpenedBuildingsListCategory);
    }

    private void OpenBuildingsListByCategory(BuildingCategory buildingCategory)
    {
        Button selectedButton = null;

        for (int i = 0; i < buildingListButtons.Count; i++)
        {
            if (i == (int)buildingCategory)
            {
                buildingLists[i].gameObject.SetActive(true);
                buildingListButtons[i].Select();
                selectedButton = buildingListButtons[i];
            }
            else
            {
                buildingLists[i].gameObject.SetActive(false);
                buildingListButtons[i].Deselect();
            }

            buildingListButtons[i].transform.SetSiblingIndex(buildingListButtons.Count - i - 1);
        }

        selectedButton.transform.SetAsLastSibling();
        lastOpenedBuildingsListCategory = buildingCategory;
        buildingListsScrollRect.content = buildingLists[(int)buildingCategory].GetComponent<RectTransform>();
    }

    private void OpenStorageMenu()
    {
        OpenManagementMenu();
        OpenStorageListsMenu();

        buildingListsMenuButton.GetComponent<RectTransform>().localScale = Vector3.one;
        storageListsMenuButton.GetComponent<RectTransform>().localScale = new Vector3(MainButton.selectedButtonUpScaleValue, MainButton.selectedButtonUpScaleValue, 1f);
    }

    private void OpenStorageListsMenu()
    {
        isBuildingListsMenuOpened = false;
        isStorageListsMenuOpened = true;

        buildingListsMenu.SetActive(false);
        storageListsMenu.SetActive(true);

        buildingListsMenuButton.Deselect();
        storageListsMenuButton.Select();

        OpenStorageListByCategory(lastOpenedStorageListCategory);
    }

    private void OpenStorageListByCategory(ItemCategory itemCategory)
    {
        Button selectedButton = null;
        int itemCategoryIndex = ((int)itemCategory) - 1;

        for (int i = 0; i < storageListButtons.Count; i++)
        {
            if (i == itemCategoryIndex)
            {
                storageLists[i].gameObject.SetActive(true);
                storageListButtons[i].Select();
                selectedButton = storageListButtons[i];
            }
            else
            {
                storageLists[i].gameObject.SetActive(false);
                storageListButtons[i].Deselect();
            }

            storageListButtons[i].transform.SetSiblingIndex(storageListButtons.Count - i - 1);
        }

        selectedButton.transform.SetAsLastSibling();
        lastOpenedStorageListCategory = itemCategory;
        //storageListsScrollRect.content = buildingLists[itemCategoryIndex].GetComponent<RectTransform>();
    }

    private void CreateBuildingWidgets()
    {
        if (!gameManager) {
            Debug.LogError("gameManager is NULL");
            return; }

        int length = Enum.GetValues(typeof(BuildingCategory)).Length;
        for (int i = 0; i < length; i++)
        {
            spawnedBuildingWidgets.Add(new List<BuildingWidget>());
        }

        for (int i = 0; i < gameManager.buildingPrefabs.Count; i++)
        {
            if (!gameManager.buildingPrefabs[i].BuildingData.IsDemolishable) continue;

            BuildingCategory buildingCategory = gameManager.buildingPrefabs[i].BuildingData.BuildingCategory;
            BuildingWidget spawnedBuildingWidget = null;
            spawnedBuildingWidget = Instantiate<BuildingWidget>(buildingWidgetPrefab, transform);

            int categoryIndex = (int)gameManager.buildingPrefabs[i].BuildingData.BuildingCategory;
            spawnedBuildingWidgets[categoryIndex].Add(spawnedBuildingWidget);

            ConstructionComponent construction = gameManager.buildingPrefabs[i].GetComponent<ConstructionComponent>();
            spawnedBuildingWidget.InitializeBuildingWidget(construction);
            spawnedBuildingWidget.cityManager = cityManager;

            spawnedBuildingWidget.transform.SetParent(buildingLists[(int)buildingCategory].transform);

            spawnedBuildingWidget.cityManager = cityManager;
        }

        for (int i = 0; i < length; i++)
        {
            RectTransform rectTransform = buildingLists[i].GetComponent<RectTransform>();
            Vector2 initialSizeDelta = rectTransform.rect.size;
            Vector2 size = buildingLists[i].transform.childCount * (buildingLists[i].cellSize + buildingLists[i].spacing) - buildingLists[i].spacing;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            if (rectTransform.sizeDelta.y < initialSizeDelta.y)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, initialSizeDelta.y);
        }
    }

    // Loot
    public void OnItemAdded(ItemInstance lootInstance)
    {
        itemsToUpdate[lootInstance.ItemData.ItemId] = true;
    }

    private void UpdateItemAmounts()
    {
        // Update Storage Menu
        for (int i = 0; i < storageResourceWidgets.Count; i++)
        {
            int amount = cityManager.items[i].Amount;
            int maxAmount = cityManager.totalStorageCapacity[i].Amount;

            if (storageResourceWidgets.Count > i)
                storageResourceWidgets[i].UpdateAmount(amount, maxAmount);
            else
                Debug.LogError("storageResourceWidgets.Count > indexes[i]");

            //if (itemsToUpdate[i] == true)
            //{
            //    itemsToUpdate[i] = false;
            //}
        }

        // Update Construction Menu
        for (int i = 0; i < spawnedBuildingWidgets.Count; i++)
        {
            for (int j = 0; j < spawnedBuildingWidgets[i].Count; j++)
            {
                spawnedBuildingWidgets[i][j].UpdateResourcesToBuild();
            }
        }
    }

    //private void UpdateStorageItemByIndex(int itemIndex, int currentResourceAmount, int maxResourceAmount)
    //{
    //    if (storageResourceWidgets.Count > itemIndex && storageResourceWidgets[itemIndex])
    //    {
    //        storageResourceWidgets[itemIndex].UpdateStorageWidget(currentResourceAmount, maxResourceAmount);
    //    }
    //}

    public void UpdateBuildingWidgetsResourcesAmount()
    {
        for (int i = 0; i < spawnedBuildingWidgets.Count; i++)
        {
            for (int j = 0; j < spawnedBuildingWidgets[i].Count; j++)
            {
                spawnedBuildingWidgets[i][j].UpdateResourcesToBuild();
            }
        }
    }

    public void OpenBuildingInformationMenu(ConstructionComponent construction)
    {
        Building building = construction.GetComponent<Building>();

        foreach (var widget in spawnedBuildingCharacteristicWidgets)
        {
            Destroy(widget.gameObject);
        }
        spawnedBuildingCharacteristicWidgets.Clear();

        buildingInformationMenu.SetActive(true);

        buildingInformationMenuNameText.SetText(building.BuildingData.BuildingName);
        if (building.levelComponent)
            buildingInformationMenuLevelNumberText.SetText("Level " + (building.levelComponent.LevelIndex + 1).ToString());
        //buildingInformationMenuDescriptionText.SetText(building.BuildingData.description);

        ProductionBuildingComponent productionBuilding = building.GetComponent<ProductionBuildingComponent>();
        StorageBuildingComponent storageBuilding = building.GetComponent<StorageBuildingComponent>();

        int index = 0;

        if (productionBuilding)
        {
            ProductionBuildingLevelData levelData = productionBuilding.levelsData[0] as ProductionBuildingLevelData;
            ItemInstance producedResource = levelData.producedResources[productionBuilding.currentProducedItemIndex].producedResource;
            CreateBuildingCharacteristicWidget("Produces", producedResource.Amount, producedResource.ItemData.ItemIcon, ref index);
            CreateBuildingCharacteristicWidget("Consumes", producedResource.Amount, producedResource.ItemData.ItemIcon, ref index);
        }

        if (storageBuilding)
        {
            StorageBuildingLevelData levelData = storageBuilding.levelsData[0] as StorageBuildingLevelData;
            CreateBuildingCharacteristicWidget("Storage capacity", levelData.storageItems[0].Amount, levelData.storageItems[0].ItemData.ItemIcon, ref index);
        }
    }

    private void CreateBuildingCharacteristicWidget(string characteristicName, int characteristicValueText, ref int index)
    {
        BuildingCharacteristicWidget productionWidget = CreateBuildingCharacteristicWidget(characteristicName, ref index);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueText);
    }

    private void CreateBuildingCharacteristicWidget(string characteristicName, Sprite characteristicValueSprite, ref int index)
    {
        BuildingCharacteristicWidget productionWidget = CreateBuildingCharacteristicWidget(characteristicName, ref index);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueSprite);
    }

    private void CreateBuildingCharacteristicWidget(string characteristicName, int characteristicValueText, Sprite characteristicValueSprite, ref int index)
    {
        BuildingCharacteristicWidget productionWidget = CreateBuildingCharacteristicWidget(characteristicName, ref index);
        productionWidget.SetCharacteristicName(characteristicName);
        productionWidget.SetCharacteristicValue(characteristicValueText, characteristicValueSprite);
    }

    private BuildingCharacteristicWidget CreateBuildingCharacteristicWidget(string characteristicName, ref int index)
    {
        BuildingCharacteristicWidget productionWidget = Instantiate(buildingCharacteristicWidget, buildingCharacteristics.transform);
        productionWidget.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -buildingCharacteristicHeight * index);
        spawnedBuildingCharacteristicWidgets.Add(productionWidget);
        index++;

        return productionWidget;
    }

    public void CloseBuildingInformationMenu()
    {
        buildingInformationMenu.SetActive(false);
    }

    // Management Menus
    public void OpenDetailsMenu(GameObject objectToShowDetails)
    {
        selectedObject = objectToShowDetails;
        isDetailsMenuOpened = true;
        isInfoMenuOpened = false;
        detailsMenu.Initialize(objectToShowDetails);
    }

    public void OnBuildingUpgraded(Building building)
    {
        //buildingManagementMenuNameText.SetText(building.buildingData.buildingName);
        //buildingManagementMenuLevelText.SetText("Level " + (building.levelIndex + 1).ToString());

        //List<int> indexes = new List<int>();

        //List<ItemInstance> previousResourcesToUpgrade = building.buildingLevelsData[building.levelIndex].resourcesToBuild;

        //for (int i = 0; i < previousResourcesToUpgrade.Count; i++)
        //    indexes.Add(previousResourcesToUpgrade[i].ItemData.ItemId);

        //UpdateStorageItemsByIndexes(indexes);

        //selectedBuilding = null;
    }

    public void CloseDetailsMenu()
	{
		isDetailsMenuOpened = false;
        isBuildingResourcesMenuOpened = false;

        //selectedBuilding = null;

        CloseBuildingActionMenu();
    }

    // Building Stats Panels
    public void OpenBuildingStatsPanel(Building building)
    {
        isInfoMenuOpened = true;
        buildingStatsPanelNameText.SetText(building.BuildingData.BuildingName);
        buildingStatsPanelWorkersCountText.SetText(building.workers.Count + "/" + building.ConstructionLevelsData[building.levelComponent.LevelIndex].maxResidentsCount);
    }

    public void CloseBuildingStatsPanel()
    {
        isInfoMenuOpened = false;
    }

    private void UpdateBuildingStatsPanelPosition()
    {
        if (isInfoMenuOpened)
        {
            buildingStatsPanel.anchoredPosition = Vector3.Lerp(buildingStatsPanel.anchoredPosition, buildingStatsPanelOpenedPosition, buildingManagementMenuToggleSpeed * Time.deltaTime);
        }
        else
        {
            buildingStatsPanel.anchoredPosition = Vector3.Lerp(buildingStatsPanel.anchoredPosition, Vector2.zero, buildingManagementMenuToggleSpeed * Time.deltaTime);
        }
    }

    // Building Action Menu
    public void OpenBuildingActionMenu()
    {
        isBuildingResourcesMenuOpened = true;
        buildingResourcesMenuBackground.gameObject.SetActive(true);
    }

    public void CloseBuildingActionMenu()
    {
        isBuildingResourcesMenuOpened = false;
        buildingResourcesMenuBackground.gameObject.SetActive(false);
    }

    public void OpenUpgradeBuildingMenu()
    {
        Building building = selectedObject.GetComponent<Building>();
        if (!building) return;

        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        int nextLevelIndex = building.levelComponent.LevelIndex + 1;
        List<ItemInstance> resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Count; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int amount = resourcesToUpgrade[i].Amount;
            resourceWidget.UpdateAmount(amount);
        }

        upgradeBuildingMenu.gameObject.SetActive(true);
        demolishBuildingMenu.gameObject.SetActive(false);
        repairBuildingMenu.gameObject.SetActive(false);
    }

    public void OpenDemolishBuildingMenu()
    {
        Building building = selectedObject.GetComponent<Building>();
        if (!building) return;

        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        int levelIndex = building.levelComponent.LevelIndex;
        List<ItemInstance> resourcesToUpgrade = building.ConstructionLevelsData[levelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Count; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int amount = (int)math.ceil(resourcesToUpgrade[i].Amount * GameManager.demolitionResourceRefundRate);
            resourceWidget.UpdateAmount(amount);
        }

        upgradeBuildingMenu.gameObject.SetActive(false);
        demolishBuildingMenu.gameObject.SetActive(true);
        repairBuildingMenu.gameObject.SetActive(false);
    }

    // Building Workers Menu
    public void OpenBuildingWorkersMenu()
    {
        Building building = selectedObject.GetComponent<Building>();
        if (!building) return;

        maxBuildingWorkersCount = building.ConstructionLevelsData[building.levelComponent.LevelIndex].maxResidentsCount;

        residentWidgetsColumnCount = (int)(buildingWorkersList.GetComponent<RectTransform>().rect.width / buildingWorkersList.cellSize.x);

        selectBuildingWorkersMenu.gameObject.SetActive(true);

        // Set parents of resident widgets
        int buildingWorkerWidgetIndex = 0;
        for (int i = 0; i < cityManager.residents.Count; i++)
        {
            spawnedResidentWidgets[i].InitializeResidentWidget(cityManager.residents[i], building, this);

            if (cityManager.residents[i].currentWork != ResidentWork.None)
            {
                if (cityManager.residents[i].workBuilding == building)
                {
                    spawnedResidentWidgets[i].transform.SetParent(buildingWorkersList.transform);
                    spawnedResidentWidgets[i].transform.SetSiblingIndex(buildingWorkerWidgetIndex);
                    buildingWorkerWidgetIndex++;
                }
                else
                {
                    spawnedResidentWidgets[i].transform.SetParent(employedResidentsList.transform);
                }
            }
            else
            {
                spawnedResidentWidgets[i].transform.SetParent(unemployedResidentsList.transform);
            }
        }

        // Create empty resident widgets
        int emptyResidentWidgetsCount = spawnedBuildingWorkerEmptyWidgets.Count;
        if (emptyResidentWidgetsCount < maxBuildingWorkersCount)
        {
            for (int i = emptyResidentWidgetsCount; i < maxBuildingWorkersCount; i++)
            {
                ResidentWidget emptyResidentWidget = Instantiate(residentWidgetPrefab);
                emptyResidentWidget.InitializeResidentWidget(null, building, this);
                spawnedBuildingWorkerEmptyWidgets.Add(emptyResidentWidget);
                emptyResidentWidget.transform.SetParent(buildingWorkersList.transform);
                emptyResidentWidget.transform.localScale = Vector3.one;
            }
        }
        else
        {
            for (int i = emptyResidentWidgetsCount - 1; i >= maxBuildingWorkersCount; i--)
            {
                Destroy(spawnedBuildingWorkerEmptyWidgets[i].gameObject);
                spawnedBuildingWorkerEmptyWidgets.RemoveAt(i);
            }
        }

        for (int i = 0; i < building.workers.Count; i++)
        {
            spawnedBuildingWorkerEmptyWidgets[i].gameObject.SetActive(false);
        }

        for (int i = building.workers.Count; i < maxBuildingWorkersCount; i++)
        {
            spawnedBuildingWorkerEmptyWidgets[i].gameObject.SetActive(true);
        }

        UpdateWorkerListsSize();
    }

    private void AddResidentWidget(Resident resident)
    {
        ResidentWidget residentWidget = Instantiate(residentWidgetPrefab, unemployedResidentsList.transform);
        residentWidget.transform.localScale = Vector3.one;
        spawnedResidentWidgets.Add(residentWidget);
    }

    private void SetWorkerListSize(RectTransform workersMenu, GridLayoutGroup gridLayoutGroup, RectTransform haveNoResidentsText, int residentsCount, int residentWidgetsColumnCount)
    {
        gridLayoutGroup.constraintCount = residentWidgetsColumnCount;
        RectTransform menuRectTransform = gridLayoutGroup.GetComponent<RectTransform>();
        int WidgetsRowCount = (int)math.ceil((float)residentsCount / (float)residentWidgetsColumnCount);
        int YSize = 0;
        Vector2 ListSize = Vector2.zero;

        //Debug.Log(residentsCount);

        if (residentsCount > 0)
        {
            YSize = (int)((menuRectTransform.offsetMin.y - menuRectTransform.offsetMax.y) + (gridLayoutGroup.cellSize.y * WidgetsRowCount) + (gridLayoutGroup.spacing.y * (WidgetsRowCount - 1)));


            if (haveNoResidentsText)
                haveNoResidentsText.gameObject.SetActive(false);
        }
        else
        {
            YSize = (int)(menuRectTransform.offsetMin.y - menuRectTransform.offsetMax.y);

            if (haveNoResidentsText)
            {
                YSize += (int)haveNoResidentsText.sizeDelta.y;
                haveNoResidentsText.gameObject.SetActive(true);
            }
        }

        workersMenu.sizeDelta = new Vector2(workersMenu.sizeDelta.x, YSize);
    }

    public void UpdateWorkerListsSize()
    {
        Building building = selectedObject.GetComponent<Building>();
        if (!building) return;

        // Building workers
        SetWorkerListSize(buildingWorkersMenu, buildingWorkersList, null, maxBuildingWorkersCount, residentWidgetsColumnCount);
        // Unemployed residents
        SetWorkerListSize(unemployedResidentsMenu, unemployedResidentsList, haveNoUnemployedResidentsText, cityManager.unemployedResidentsCount, residentWidgetsColumnCount);
        // Employed residents
        SetWorkerListSize(employedResidentsMenu, employedResidentsList, haveNoEmployedResidentsText, cityManager.employedResidentCount - building.workers.Count, residentWidgetsColumnCount);
    }

    public void SelectBuildingWorker(ResidentWidget residentWidget)
    {
        Building building = selectedObject.GetComponent<Building>();
        if (!building) return;

        Resident resident = residentWidget.resident;
        int workersCount = building.workers.Count;

        if (resident.currentWork != ResidentWork.None)
        {
            if (resident.workBuilding == building)
            {
                residentWidget.transform.SetParent(buildingWorkersList.transform);
                residentWidget.transform.SetSiblingIndex(workersCount - 1);

                int index = building.workers.Count - 1;
                spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(false);
            }
            else
            {
                residentWidget.transform.SetParent(employedResidentsList.transform);

                if (workersCount < maxBuildingWorkersCount)
                {
                    int index = maxBuildingWorkersCount - (maxBuildingWorkersCount - workersCount);
                    spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(true);
                }
            }
        }
        else
        {
            residentWidget.transform.SetParent(unemployedResidentsList.transform);

            if (workersCount < maxBuildingWorkersCount)
            {
                int index = maxBuildingWorkersCount - (maxBuildingWorkersCount - workersCount);
                spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(true);
            }
        }

        UpdateWorkerListsSize();
    }

    private void CloseBuildingWorkersMenu()
    {
        selectBuildingWorkersMenu.gameObject.SetActive(false);
    }

    // Repair Building Menu
    public void OpenRepairBuildingMenu(Building building)
    {
        CloseDetailsMenu();
        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        selectedObject = building.gameObject;

        repairBuildingNameText.SetText(building.BuildingData.BuildingName + " (Ruin)");

        int nextLevelIndex = 0;
        List<ItemInstance> resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Count; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int amount = resourcesToUpgrade[i].Amount;
            resourceWidget.UpdateAmount(amount);
        }

        upgradeBuildingMenu.gameObject.SetActive(false);
        demolishBuildingMenu.gameObject.SetActive(false);
        repairBuildingMenu.gameObject.SetActive(true);
    }

    // Upgrade Building Menu
    private void TryToUpgradeBuilding()
    {
        cityManager.TryToUpgradeConstruction(selectedObject.GetComponent<Building>());
    }

    private void TryToDemolishBuilding()
    {
        OpenBuildingActionMenu();

        //selectedBuilding.OnConstructionDemolised();


        CloseDetailsMenu();
    }

    private void CleanResourceToUpgradeWidgets()
    {
        for (int i = 0; i < spawnedBuildingActionResourceWidgets.Count; i++)
        {
            Destroy(spawnedBuildingActionResourceWidgets[i].gameObject);
        }

        spawnedBuildingActionResourceWidgets.Clear();
    }

    // Placing Building
    private void OnConstructionStartPlacing(ConstructionComponent building)
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(true);
        else
            Debug.Log("stopPlacingBuildingButton is NULL");

        CloseManagementMenu();
    }

    private void OnConstructionPlaced()
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(false);
    }

    private void StopPlacingBuilding()
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(false);

        OnBuildStopPlacing?.Invoke();
    }

    private void FPSCounter()
    {
        frames++;
        double time = Time.timeAsDouble;

        if (time >= lastFPSCounterTime + 0.5)
        {
            double delta = time - lastFPSCounterTime;
            float fps = frames / (float)delta;
            fpsText.text = $"FPS: {(int)fps}";
            frames = 0;
            lastFPSCounterTime = time;
        }
    }
}
