using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PlayerController playerController;
    private GameManager gameManager;
    private CityManager cityManager;
    public Building selectedBuilding { get; private set; } = null;

    // Widgets
    [Header("Widgets")]
    [SerializeField] private BuildingWidget buildingWidgetPrefab = null;
    [SerializeField] private ResourceWidget storageResourceWidgetPrefab = null;
    [SerializeField] private ResourceWidget buildingActionResourceWidgetPrefab = null;
    private List<List<BuildingWidget>> buildingWidgets = new List<List<BuildingWidget>>();
    [SerializeField] private List<ResourceWidget> storageResourceWidgets = new List<ResourceWidget>();
    private List<ResourceWidget> buildingActionResourceWidgets = new List<ResourceWidget>();

    // Menus
    [Header("Menus")]
    [SerializeField] private GameObject managementMenu = null;
    [SerializeField] private GameObject buildingListsMenu = null;
    [SerializeField] private GameObject storageListsMenu = null;
    private bool isManagementMenuOpened = false;
    private bool isBuildingListsMenuOpened = false;
    private bool isStorageListsMenuOpened = false;
    private bool isBuildingManagementMenuOpened = false;

    BuildingCategory lastOpenedBuildingsListCategory = BuildingCategory.Construction;
    ItemCategory lastOpenedStorageListCategory = ItemCategory.Building;

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

    // Storage List
    [Header("Storage List")]
    [SerializeField] private GridLayoutGroup buildingResourcesList = null;
    [SerializeField] private GridLayoutGroup craftingResourcesList = null;
    [SerializeField] private GridLayoutGroup weaponResourcesList = null;

    // Storage List
    [Header("Storage List Buttons")]
    [SerializeField] private Button buildingResourcesListButton = null;
    [SerializeField] private Button craftingResourcesListButton = null;
    [SerializeField] private Button weaponResourcesListButton = null;

    // Building Management Menu
    [Header("Building Management Menu")]
    [SerializeField] private RectTransform buildingManagementMenu = null;
    [SerializeField] private RectTransform buildingManagementMenuPanel = null;
    private Vector2 buildingManagementMenuCurrentPosition = Vector2.zero;
    private const float buildingManagementMenuToggleSpeed = 15.0f;

    private GameObject spawnedBuildingManagementMenu = null;
    [SerializeField] private TextMeshProUGUI buildingManagementMenuNameText = null;
    [SerializeField] private TextMeshProUGUI buildingManagementMenuLevelText = null;
    [SerializeField] private Button closeBuildingManagementMenuButton = null;
    [SerializeField] private Button showUpgradeBuildingResourcesButton = null;
    [SerializeField] private Button showDemolishBuildingResourcesButton = null;
    [SerializeField] private Button openBuildingWorkersMenuButton = null;
    [SerializeField] private Button closeBuildingWorkersMenuButton = null;

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

    int maxBuildingWorkersCount = 0;
    int residentWidgetsColumnCount = 0;

    // Colors
    [Header("Colors")]
    [SerializeField] private UIColor blueColor;
    [SerializeField] private UIColor lightBlueColor;
    [SerializeField] private UIColor darkBlueColor;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    private void Start()
    {
        InitializeUIManager();
    }

    private void OnEnable()
    {
        //Building.OnBuildingFinishConstructing += OnBuildingUpgraded;
        cityManager.OnResidentAdded += AddResidentWidget;
    }

    private void OnDisable()
    {
        //Building.OnBuildingFinishConstructing -= OnBuildingUpgraded;
        cityManager.OnResidentAdded -= AddResidentWidget;
    }

    private void Update()
    {
        // Building Management Menu
        if (isBuildingManagementMenuOpened)
            buildingManagementMenuCurrentPosition.y = math.lerp(buildingManagementMenuCurrentPosition.y, buildingManagementMenu.rect.size.y, buildingManagementMenuToggleSpeed * Time.deltaTime);
        else
            buildingManagementMenuCurrentPosition.y = math.lerp(buildingManagementMenuCurrentPosition.y, 0, buildingManagementMenuToggleSpeed * Time.deltaTime);

		buildingManagementMenu.anchoredPosition = buildingManagementMenuCurrentPosition;

        if (isBuildingResourcesMenuOpened)
            buildingResourcesMenuCurrentPosition.y = math.lerp(buildingResourcesMenuCurrentPosition.y, buildingResourcesMenuOpenedPosition.y, buildingResourcesMenuPanelToggleSpeed * Time.deltaTime);
        else
            buildingResourcesMenuCurrentPosition.y = math.lerp(buildingResourcesMenuCurrentPosition.y, 0, buildingResourcesMenuPanelToggleSpeed * Time.deltaTime);

        buildingResourcesMenuPanel.anchoredPosition = buildingResourcesMenuCurrentPosition;
    }

    public void InitializeUIManager()
    {
        CreateBuildingWidgets();
        CreateItemWidgets();

        buildingResourcesMenuOpenedPosition.y = Screen.height / 2 + (buildingResourcesMenuPanel.rect.size.y / 2);
        buildingResourcesMenuCurrentPosition.y = 0;

        buildingMenuButton.onClick.AddListener(OpenBuildingMenu);
        storageMenuButton.onClick.AddListener(OpenStorageMenu);

        buildingListsMenuButton.onClick.AddListener(OpenBuildingListsMenu);
        storageListsMenuButton.onClick.AddListener(OpenStorageListsMenu);

        closeManagementMenuButton.onClick.AddListener(CloseManagementMenu);

        stopPlacingBuildingButton.onClick.AddListener(StopPlacingBuilding);

        // Building List Buttons
        System.Array buildingCategoriesEnum = System.Enum.GetValues(typeof(BuildingCategory));
        for (int i = 0; i < buildingCategoriesEnum.Length; i++)
        {
            int index = i;
            buildingListButtons[index].onClick.AddListener(() => OpenBuildingsListByCategory((BuildingCategory)buildingCategoriesEnum.GetValue(index)));
        }

        // Resource Lists
        buildingResourcesListButton.onClick.AddListener(() => OpenStorageListByCategory(ItemCategory.Building));
        craftingResourcesListButton.onClick.AddListener(() => OpenStorageListByCategory(ItemCategory.Crafting));
        weaponResourcesListButton.onClick.AddListener(() => OpenStorageListByCategory(ItemCategory.Weapon));

        closeBuildingManagementMenuButton.onClick.AddListener(CloseBuildingManagementMenu);
        showUpgradeBuildingResourcesButton.onClick.AddListener(OpenUpgradeBuildingMenu);
        showDemolishBuildingResourcesButton.onClick.AddListener(OpenDemolishBuildingMenu);
        openBuildingWorkersMenuButton.onClick.AddListener(OpenBuildingWorkersMenu);
        closeBuildingWorkersMenuButton.onClick.AddListener(CloseBuildingWorkersMenu);

        closeBuildingResourcesMenuButton.onClick.AddListener(CloseBuildingActionMenu);
        closeBuildingResourcesMenuBackgroundButton.onClick.AddListener(CloseBuildingActionMenu);

        upgradeBuildingButton.onClick.AddListener(TryToUpgradeBuilding);
        demolishBuildingButton.onClick.AddListener(TryToDemolishBuilding);
        repairBuildingButton.onClick.AddListener(TryToUpgradeBuilding);

        managementMenu.SetActive(false);
        buildingListsMenu.SetActive(false);
        selectBuildingWorkersMenu.gameObject.SetActive(false);
        stopPlacingBuildingButton.gameObject.SetActive(false);
    }

    // Management Menu
    private void OpenManagementMenu()
    {
        isManagementMenuOpened = true;
        managementMenu.SetActive(true);
        UpdateBuildingWidgetsResourcesAmount();
    }

    private void OpenBuildingMenu()
    {
        OpenManagementMenu();
        OpenBuildingListsMenu();
        OpenBuildingsListByCategory(lastOpenedBuildingsListCategory);

        buildingListsMenuButton.GetComponent<RectTransform>().localScale = new Vector3(MainButton.selectedButtonUpScaleValue, MainButton.selectedButtonUpScaleValue, 1f);
        storageListsMenuButton.GetComponent<RectTransform>().localScale = Vector3.one;

        buildingListButtons[(int)lastOpenedBuildingsListCategory].GetComponent<RectTransform>().localScale = new Vector3(MainButton.selectedButtonUpScaleValue, MainButton.selectedButtonUpScaleValue, 1f);
    }

    private void OpenStorageMenu()
    {
        OpenManagementMenu();
        OpenStorageListsMenu();
        OpenStorageListByCategory(lastOpenedStorageListCategory);

        buildingListsMenuButton.GetComponent<RectTransform>().localScale = Vector3.one;
        storageListsMenuButton.GetComponent<RectTransform>().localScale = new Vector3(MainButton.selectedButtonUpScaleValue, MainButton.selectedButtonUpScaleValue, 1f);
    }

    private void OpenBuildingListsMenu()
    {
        isBuildingListsMenuOpened = true;
        isStorageListsMenuOpened = false;

        buildingListsMenu.SetActive(true);
        storageListsMenu.SetActive(false);

        buildingListsMenuButton.Select();
        storageListsMenuButton.Deselect();

        OpenBuildingsListByCategory(lastOpenedBuildingsListCategory);
    }

    private void OpenStorageListsMenu()
    {
        isStorageListsMenuOpened = true;
        isBuildingListsMenuOpened = false;

        storageListsMenu.SetActive(true);
        buildingListsMenu.SetActive(false);
        
        buildingListsMenuButton.Deselect();
        storageListsMenuButton.Select();
    }

    public void CloseManagementMenu()
    {
        isManagementMenuOpened = false;
        managementMenu.SetActive(false);
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

    private void OpenStorageListByCategory(ItemCategory itemCategory)
    {
        buildingResourcesList.gameObject.SetActive(itemCategory == ItemCategory.Building ? true : false);
        craftingResourcesList.gameObject.SetActive(itemCategory == ItemCategory.Crafting ? true : false);
        weaponResourcesList.gameObject.SetActive(itemCategory == ItemCategory.Weapon ? true : false);

        lastOpenedStorageListCategory = itemCategory;
    }

    private void CreateBuildingWidgets()
    {
        int length = Enum.GetValues(typeof(BuildingCategory)).Length;
        for (int i = 0; i < length; i++)
        {
            buildingWidgets.Add(new List<BuildingWidget>());
        }

        for (int i = 0; i < gameManager.buildingPrefabs.Count; i++)
        {
            if (!gameManager.buildingPrefabs[i].buildingData.isDemolishable) continue;

            BuildingCategory buildingCategory = gameManager.buildingPrefabs[i].buildingData.buildingCategory;
            BuildingWidget spawnedBuildingWidget = null;
            spawnedBuildingWidget = Instantiate<BuildingWidget>(buildingWidgetPrefab, transform);

            int categoryIndex = (int)gameManager.buildingPrefabs[i].buildingData.buildingCategory;
            buildingWidgets[categoryIndex].Add(spawnedBuildingWidget);

            spawnedBuildingWidget.InitializeBuildingWidget(gameManager.buildingPrefabs[i]);
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

    private void CreateItemWidgets()
    {
        for (int i = 0; i < cityManager.items.Count; i++)
        {
            //string itemName = gameManager.buildingPrefabs[i].buildingData.buildingIdName;
            if (cityManager.items[i].itemData.itemCategory == ItemCategory.Society) continue;

            ItemCategory itemCategory = cityManager.items[i].itemData.itemCategory;

            ResourceWidget storageResourceWidget = Instantiate(storageResourceWidgetPrefab);
            storageResourceWidgets.Add(storageResourceWidget);

            int itemAmount = cityManager.items[i].amount;
            int itemMaxAmount = cityManager.items[i].maxAmount;
            storageResourceWidget.UpdateStorageWidget(itemAmount, itemMaxAmount);

            if (itemCategory == ItemCategory.Building)
                storageResourceWidget.transform.SetParent(buildingResourcesList.transform);
            else if (itemCategory == ItemCategory.Crafting)
                storageResourceWidget.transform.SetParent(craftingResourcesList.transform);
            else if (itemCategory == ItemCategory.Weapon)
                storageResourceWidget.transform.SetParent(weaponResourcesList.transform);
        }
    }

    public void UpdateStorageItemsByIndexes(List<int> indexes)
    {
        for (int i = 0; i < indexes.Count; i++)
        {
            int amount = cityManager.items[indexes[i]].amount;
            int maxAmount = cityManager.items[indexes[i]].maxAmount;

            if (storageResourceWidgets.Count > indexes[i])
                storageResourceWidgets[indexes[i]].UpdateStorageWidget(amount, maxAmount);
            else
                Debug.LogError("storageResourceWidgets.Count > indexes[i]");
        }
    }

    public void UpdateStorageItemByIndex(int itemIndex, int currentResourceAmount, int maxResourceAmount)
    {
        if (storageResourceWidgets.Count > itemIndex && storageResourceWidgets[itemIndex])
        {
            storageResourceWidgets[itemIndex].UpdateStorageWidget(currentResourceAmount, maxResourceAmount);
        }
    }

    public void UpdateBuildingWidgetsResourcesAmount()
    {
        for (int i = 0; i < buildingWidgets.Count; i++)
        {
            for (int j = 0; j < buildingWidgets[i].Count; j++)
            {
                buildingWidgets[i][j].UpdateResourcesToBuild();
            }
        }
    }

    // Building Management Menu
    public void OpenBuildingManagementMenu(Building building)
    {
		isBuildingManagementMenuOpened = true;
        selectedBuilding = building;

        buildingManagementMenuNameText.SetText(building.buildingData.buildingName);
        buildingManagementMenuLevelText.SetText("Level " + (building.levelIndex + 1).ToString());

        if (spawnedBuildingManagementMenu)
            Destroy(spawnedBuildingManagementMenu);

        if (building.buildingData.buildingManagementMenuWidget)
            spawnedBuildingManagementMenu = Instantiate(building.buildingData.buildingManagementMenuWidget, buildingManagementMenuPanel.transform);

        if (building.buildingData.isDemolishable)
        {
            showDemolishBuildingResourcesButton.interactable = true;
        }
        else
        {
            showDemolishBuildingResourcesButton.interactable = false;
        }
    }

    public void OnBuildingUpgraded(Building building)
    {
        buildingManagementMenuNameText.SetText(building.buildingData.buildingName);
        buildingManagementMenuLevelText.SetText("Level " + (building.levelIndex + 1).ToString());

        List<int> indexes = new List<int>();

        List<ResourceToBuild> previousResourcesToUpgrade = building.buildingLevelsData[building.levelIndex].ResourcesToBuild;

        for (int i = 0; i < previousResourcesToUpgrade.Count; i++)
            indexes.Add(gameManager.GetItemIndexByIdName(previousResourcesToUpgrade[i].resourceData.itemIdName));

        UpdateStorageItemsByIndexes(indexes);

        //selectedBuilding = null;
    }

    public void CloseBuildingManagementMenu()
	{
		isBuildingManagementMenuOpened = false;
        isBuildingResourcesMenuOpened = false;

        //selectedBuilding = null;

        CloseBuildingActionMenu();
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

    private void OpenUpgradeBuildingMenu()
    {
        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        int nextLevelIndex = selectedBuilding.levelIndex + 1;
        List<ResourceToBuild> resourcesToUpgrade = selectedBuilding.buildingLevelsData[nextLevelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Count; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            buildingActionResourceWidgets.Add(resourceWidget);

            int amount = resourcesToUpgrade[i].amount;
            resourceWidget.UpdateBuildWidget(amount);
        }

        upgradeBuildingMenu.gameObject.SetActive(true);
        demolishBuildingMenu.gameObject.SetActive(false);
        repairBuildingMenu.gameObject.SetActive(false);
    }

    private void OpenDemolishBuildingMenu()
    {
        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        int levelIndex = selectedBuilding.levelIndex;
        List<ResourceToBuild> resourcesToUpgrade = selectedBuilding.buildingLevelsData[levelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Count; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            buildingActionResourceWidgets.Add(resourceWidget);

            int amount = (int)math.ceil(resourcesToUpgrade[i].amount * GameManager.demolitionResourceRefundRate);
            resourceWidget.UpdateBuildWidget(amount);
        }

        upgradeBuildingMenu.gameObject.SetActive(false);
        demolishBuildingMenu.gameObject.SetActive(true);
        repairBuildingMenu.gameObject.SetActive(false);
    }

    // Building Workers Menu
    private void OpenBuildingWorkersMenu()
    {
        maxBuildingWorkersCount = selectedBuilding.buildingLevelsData[selectedBuilding.levelIndex].maxResidentsCount;

        residentWidgetsColumnCount = (int)(buildingWorkersList.GetComponent<RectTransform>().rect.width / buildingWorkersList.cellSize.x);

        selectBuildingWorkersMenu.gameObject.SetActive(true);

        // Set parents of resident widgets
        int buildingWorkerWidgetIndex = 0;
        Debug.Log(cityManager.residents.Count);
        for (int i = 0; i < cityManager.residents.Count; i++)
        {
            spawnedResidentWidgets[i].InitializeResidentWidget(cityManager.residents[i], selectedBuilding, this);

            if (cityManager.residents[i].isWorker)
            {
                if (cityManager.residents[i].workBuilding == selectedBuilding)
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
                emptyResidentWidget.InitializeResidentWidget(null, selectedBuilding, this);
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

        for (int i = 0; i < selectedBuilding.workers.Count; i++)
        {
            spawnedBuildingWorkerEmptyWidgets[i].gameObject.SetActive(false);
        }

        for (int i = selectedBuilding.workers.Count; i < maxBuildingWorkersCount; i++)
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
        // Building workers
        SetWorkerListSize(buildingWorkersMenu, buildingWorkersList, null, maxBuildingWorkersCount, residentWidgetsColumnCount);
        // Unemployed residents
        SetWorkerListSize(unemployedResidentsMenu, unemployedResidentsList, haveNoUnemployedResidentsText, cityManager.unemployedResidentsCount, residentWidgetsColumnCount);
        // Employed residents
        SetWorkerListSize(employedResidentsMenu, employedResidentsList, haveNoEmployedResidentsText, cityManager.employedResidentCount - selectedBuilding.workers.Count, residentWidgetsColumnCount);
    }

    public void SelectBuildingWorker(ResidentWidget residentWidget)
    {
        Resident resident = residentWidget.resident;
        int workersCount = selectedBuilding.workers.Count;

        if (resident.isWorker)
        {
            if (resident.workBuilding == selectedBuilding)
            {
                residentWidget.transform.SetParent(buildingWorkersList.transform);
                residentWidget.transform.SetSiblingIndex(workersCount - 1);

                int index = selectedBuilding.workers.Count - 1;
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
        CloseBuildingManagementMenu();
        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        selectedBuilding = building;

        repairBuildingNameText.SetText(building.buildingData.buildingName + " (Ruin)");

        int nextLevelIndex = 0;
        List<ResourceToBuild> resourcesToUpgrade = building.buildingLevelsData[nextLevelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Count; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            buildingActionResourceWidgets.Add(resourceWidget);

            int amount = resourcesToUpgrade[i].amount;
            resourceWidget.UpdateBuildWidget(amount);
        }

        upgradeBuildingMenu.gameObject.SetActive(false);
        demolishBuildingMenu.gameObject.SetActive(false);
        repairBuildingMenu.gameObject.SetActive(true);
    }

    // Upgrade Building Menu
    private void TryToUpgradeBuilding()
    {
        cityManager.TryToUpgradeBuilding(selectedBuilding);
    }

    private void TryToDemolishBuilding()
    {
        OpenBuildingActionMenu();

        selectedBuilding.Demolish();


        CloseBuildingManagementMenu();
    }

    private void CleanResourceToUpgradeWidgets()
    {
        for (int i = 0; i < buildingActionResourceWidgets.Count; i++)
        {
            Destroy(buildingActionResourceWidgets[i].gameObject);
        }

        buildingActionResourceWidgets.Clear();
    }

    // Placing Building
    public void OnBuildingPlacingStarted()
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(true);
        else
            Debug.Log("stopPlacingBuildingButton is NULL");
    }

    public void OnBuildingPlacingStopped()
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(false);
        else
            Debug.Log("stopPlacingBuildingButton is NULL");
    }

    private void StopPlacingBuilding()
    {
        playerController.StopPlacingBuilding(playerController.buildingToPlace);
    }
}
